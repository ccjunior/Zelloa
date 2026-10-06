using Microsoft.AspNetCore.Identity;
using Zelloa.Application.Identity;
using Zelloa.Infrastructure.Identity;

namespace Zelloa.Api.Identity;

public static class PlatformAdminBootstrapper
{
    public static async Task BootstrapPlatformAdminAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        var email = configuration["IdentityBootstrap:PlatformAdmin:Email"];
        var password = configuration["IdentityBootstrap:PlatformAdmin:Password"];
        var displayName = configuration["IdentityBootstrap:PlatformAdmin:DisplayName"];

        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Both IdentityBootstrap:PlatformAdmin:Email and Password must be configured to bootstrap the platform administrator.");
        }

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ZelloaUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        if (!await roleManager.RoleExistsAsync(ZelloaRoles.PlatformAdmin))
        {
            throw new InvalidOperationException(
                "The Identity roles migration must be applied before bootstrapping the platform administrator.");
        }

        var platformAdmins = await userManager.GetUsersInRoleAsync(ZelloaRoles.PlatformAdmin);
        if (platformAdmins.Count > 0)
        {
            return;
        }

        var normalizedEmail = email.Trim();
        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "The configured bootstrap email already belongs to an account that is not a platform administrator.");
        }

        var user = new ZelloaUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? normalizedEmail : displayName.Trim(),
            TenantId = null,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Platform administrator bootstrap failed: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, ZelloaRoles.PlatformAdmin);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException(
                $"Platform administrator role assignment failed: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
        }
    }
}
