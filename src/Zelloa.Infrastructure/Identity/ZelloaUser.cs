using Microsoft.AspNetCore.Identity;
using Zelloa.Domain.Tenants;

namespace Zelloa.Infrastructure.Identity;

public sealed class ZelloaUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public Guid? TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public override bool LockoutEnabled { get; set; } = true;
}
