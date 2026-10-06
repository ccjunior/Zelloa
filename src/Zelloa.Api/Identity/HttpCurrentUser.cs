using System.Security.Claims;
using Zelloa.Application.Identity;

namespace Zelloa.Api.Identity;

public sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public Guid? UserId => Guid.TryParse(
        Principal.FindFirstValue(ClaimTypes.NameIdentifier),
        out var userId)
        ? userId
        : null;

    public string? DisplayName => Principal.FindFirstValue(ZelloaClaimTypes.DisplayName);

    public Guid? TenantId => Guid.TryParse(
        Principal.FindFirstValue(ZelloaClaimTypes.TenantId),
        out var tenantId)
        ? tenantId
        : null;

    public bool IsAuthenticated => Principal.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles => Principal.FindAll(ClaimTypes.Role)
        .Select(claim => claim.Value)
        .ToArray();
}
