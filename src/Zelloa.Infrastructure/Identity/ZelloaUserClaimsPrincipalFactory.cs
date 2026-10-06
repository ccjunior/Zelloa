using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Zelloa.Application.Identity;

namespace Zelloa.Infrastructure.Identity;

public sealed class ZelloaUserClaimsPrincipalFactory(
    UserManager<ZelloaUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<ZelloaUser, IdentityRole<Guid>>(
        userManager,
        roleManager,
        optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ZelloaUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ZelloaClaimTypes.DisplayName, user.DisplayName));

        if (user.TenantId is { } tenantId)
        {
            identity.AddClaim(new Claim(ZelloaClaimTypes.TenantId, tenantId.ToString()));
        }

        return identity;
    }
}
