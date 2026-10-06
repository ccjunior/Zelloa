using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Zelloa.Application.Identity;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Infrastructure.Identity;

namespace Zelloa.Api.Identity;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/api/auth").WithTags("Identity");

        auth.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
            {
                context.Response.Headers.CacheControl = "no-store";
                var tokens = antiforgery.GetAndStoreTokens(context);
                return Results.Ok(new CsrfTokenResponse(tokens.RequestToken!));
            })
            .AllowAnonymous()
            .WithName("GetAntiforgeryToken");

        auth.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/activate", ActivateAsync)
            .AllowAnonymous()
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("ActivateInvitedAccount")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        endpoints.MapGet("/api/me", (HttpContext context, ICurrentUser currentUser) =>
            {
                context.Response.Headers.CacheControl = "no-store";
                return Results.Ok(new CurrentUserResponse(
                    currentUser.UserId!.Value,
                    currentUser.DisplayName ?? string.Empty,
                    currentUser.TenantId,
                    currentUser.Roles));
            })
            .RequireAuthorization()
            .WithTags("Identity")
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext context,
        IAntiforgery antiforgery,
        UserManager<ZelloaUser> userManager,
        SignInManager<ZelloaUser> signInManager)
    {
        context.Response.Headers.CacheControl = "no-store";

        if (!await IsAntiforgeryValidAsync(context, antiforgery))
        {
            return Results.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || request.Email.Length > 256
            || request.Password.Length > 256)
        {
            return Results.BadRequest();
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var validTenantRole = user.TenantId is not null
            && roles.Count > 0
            && roles.All(ZelloaRoles.IsInstitutional);
        var validPlatformRole = user.TenantId is null
            && roles.Count == 1
            && roles[0] == ZelloaRoles.PlatformAdmin;
        if (!validTenantRole && !validPlatformRole)
        {
            return Results.Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        return result.Succeeded ? Results.NoContent() : Results.Unauthorized();
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<ZelloaUser> signInManager)
    {
        context.Response.Headers.CacheControl = "no-store";

        if (!await IsAntiforgeryValidAsync(context, antiforgery))
        {
            return Results.BadRequest();
        }

        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> ActivateAsync(
        ActivateAccountRequest request,
        HttpContext context,
        IdentityInvitationService invitations,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        if (string.IsNullOrWhiteSpace(request.Email)
            || request.Email.Length > 256
            || string.IsNullOrWhiteSpace(request.Token)
            || request.Token.Length > 256
            || string.IsNullOrEmpty(request.Password)
            || request.Password.Length > 256)
        {
            return Results.BadRequest();
        }

        var result = await invitations.ActivateAsync(
            request.Email, request.Token, request.Password, cancellationToken);
        if (result.Status == ActivationStatus.Rejected) return Results.Unauthorized();
        if (result.Status == ActivationStatus.InvalidPassword)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["password"] = result.Errors.ToArray()
            });
        }

        return Results.NoContent();
    }

    private static async Task<bool> IsAntiforgeryValidAsync(
        HttpContext context,
        IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }

    public sealed record CurrentUserResponse(
        Guid UserId,
        string DisplayName,
        Guid? TenantId,
        IReadOnlyCollection<string> Roles);
}

public sealed record CsrfTokenResponse(string RequestToken);

public sealed record LoginRequest(string Email, string Password);
public sealed record ActivateAccountRequest(string Email, string Token, string Password);
