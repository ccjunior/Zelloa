using System.Security.Cryptography;
using System.Text;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Zelloa.Application.Identity;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Domain.Guardians;
using Zelloa.Domain.Invitations;
using Zelloa.Infrastructure.Identity;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.Api.Identity;

public sealed class IdentityInvitationService(
    ZelloaDbContext dbContext,
    ISchoolAcademicStore schoolStore,
    UserManager<ZelloaUser> userManager,
    TimeProvider timeProvider,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    private static readonly TimeSpan InvitationLifetime = TimeSpan.FromHours(24);

    public async Task<InvitationResult> InviteSchoolAdminAsync(
        Guid schoolId, string displayName, string email, CancellationToken cancellationToken)
    {
        var school = await schoolStore.GetSchoolIgnoringTenantFilterAsync(schoolId, cancellationToken);
        if (school is null) return InvitationResult.NotFoundResult;
        return await CreateInvitationAsync(
            school.TenantId,
            ZelloaRoles.SchoolAdmin,
            displayName,
            email,
            "School",
            guardian: false,
            cancellationToken);
    }

    public Task<InvitationResult> InviteGuardianAsync(
        Guid tenantId, string displayName, string email, CancellationToken cancellationToken) =>
        CreateInvitationAsync(
            tenantId,
            ZelloaRoles.Guardian,
            displayName,
            email,
            "Family",
            guardian: true,
            cancellationToken);

    public async Task<InvitationResult> RenewSchoolAdminInvitationAsync(
        string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null) return InvitationResult.NotFoundResult;

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Contains(ZelloaRoles.SchoolAdmin) || user.TenantId is not { } tenantId)
            return InvitationResult.NotFoundResult;

        if (user.EmailConfirmed || user.PasswordHash is not null)
            return InvitationResult.ConflictResult;

        var hasSchool = await dbContext.Schools.IgnoreQueryFilters()
            .AnyAsync(school => school.TenantId == tenantId, cancellationToken);
        if (!hasSchool) return InvitationResult.NotFoundResult;

        var clientUrl = configuration["InvitationUrls:School"];
        if (!Uri.TryCreate(clientUrl, UriKind.Absolute, out var activationBase)
            || (environment.IsProduction() && activationBase.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("InvitationUrls:School must be configured with an absolute URL.");
        }

        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(InvitationLifetime);
        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var activationUrl = CreateActivationUrl(activationBase.ToString(), user.Email!, rawToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await dbContext.Entry(user).ReloadAsync(cancellationToken);
        if (user.EmailConfirmed || user.PasswordHash is not null)
            return InvitationResult.ConflictResult;

        await dbContext.AccountInvitations
            .Where(invitation => invitation.UserId == user.Id && invitation.RedeemedAt == null)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.AccountInvitations.Add(new AccountInvitation(
            Guid.NewGuid(),
            user.Id,
            tenantId,
            ZelloaRoles.SchoolAdmin,
            HashToken(rawToken),
            now,
            expiresAt));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return InvitationResult.Created(user.Id, activationUrl, expiresAt);
    }

    public async Task<ActivationResult> ActivateAsync(
        string email, string token, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = userManager.NormalizeEmail(email.Trim());
        var tokenHash = HashToken(token);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var invitation = await dbContext.AccountInvitations.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.TokenHash == tokenHash, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (invitation is null || invitation.ExpiresAt <= now)
            return ActivationResult.Rejected;

        var user = await userManager.FindByIdAsync(invitation.UserId.ToString());
        if (user is null
            || userManager.NormalizeEmail(user.Email) != normalizedEmail
            || user.EmailConfirmed
            || user.PasswordHash is not null)
        {
            return ActivationResult.Rejected;
        }

        var redeemed = await dbContext.AccountInvitations
            .Where(candidate => candidate.Id == invitation.Id
                && candidate.RedeemedAt == null
                && candidate.ExpiresAt > now)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(candidate => candidate.RedeemedAt, now),
                cancellationToken);
        if (redeemed != 1) return ActivationResult.Rejected;

        var passwordResult = await userManager.AddPasswordAsync(user, password);
        if (!passwordResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ActivationResult.InvalidPassword(passwordResult.Errors.Select(error => error.Description).ToArray());
        }

        user.EmailConfirmed = true;
        var confirmationResult = await userManager.UpdateAsync(user);
        if (!confirmationResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ActivationResult.Rejected;
        }

        await transaction.CommitAsync(cancellationToken);
        return ActivationResult.Success;
    }

    private async Task<InvitationResult> CreateInvitationAsync(
        Guid tenantId,
        string role,
        string displayName,
        string email,
        string client,
        bool guardian,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
            return InvitationResult.ConflictResult;

        var clientUrl = configuration[$"InvitationUrls:{client}"];
        if (!Uri.TryCreate(clientUrl, UriKind.Absolute, out var activationBase)
            || (environment.IsProduction() && activationBase.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"InvitationUrls:{client} must be configured with an absolute URL.");
        }

        var now = timeProvider.GetUtcNow();
        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var activationUrl = CreateActivationUrl(activationBase.ToString(), normalizedEmail, rawToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var user = new ZelloaUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = displayName.Trim(),
            TenantId = tenantId,
            EmailConfirmed = false,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return InvitationResult.ConflictResult;
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return InvitationResult.Misconfigured;
        }

        if (guardian)
            dbContext.Guardians.Add(new Guardian(user.Id, tenantId, displayName));

        dbContext.AccountInvitations.Add(new AccountInvitation(
            Guid.NewGuid(),
            user.Id,
            tenantId,
            role,
            HashToken(rawToken),
            now,
            now.Add(InvitationLifetime)));

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return InvitationResult.Created(user.Id, activationUrl, now.Add(InvitationLifetime));
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string CreateActivationUrl(string activationBase, string email, string token) =>
        QueryHelpers.AddQueryString(activationBase, new Dictionary<string, string?>
        {
            ["email"] = email,
            ["token"] = token
        });
}

public sealed record InvitationResult(InvitationStatus Status, Guid? UserId, string? ActivationUrl, DateTimeOffset? ExpiresAt)
{
    public bool Succeeded => Status == InvitationStatus.Created;
    public static InvitationResult Created(Guid userId, string url, DateTimeOffset expiresAt) =>
        new(InvitationStatus.Created, userId, url, expiresAt);
    public static InvitationResult NotFoundResult => new(InvitationStatus.NotFound, null, null, null);
    public static InvitationResult ConflictResult => new(InvitationStatus.Conflict, null, null, null);
    public static InvitationResult Misconfigured => new(InvitationStatus.Misconfigured, null, null, null);
}

public enum InvitationStatus { Created, NotFound, Conflict, Misconfigured }

public sealed record ActivationResult(ActivationStatus Status, IReadOnlyList<string> Errors)
{
    public bool Succeeded => Status == ActivationStatus.Activated;
    public static ActivationResult Success => new(ActivationStatus.Activated, []);
    public static ActivationResult Rejected => new(ActivationStatus.Rejected, []);
    public static ActivationResult InvalidPassword(IReadOnlyList<string> errors) => new(ActivationStatus.InvalidPassword, errors);
}

public enum ActivationStatus { Activated, Rejected, InvalidPassword }
