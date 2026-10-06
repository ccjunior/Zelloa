using Zelloa.Application.Identity;

namespace Zelloa.Api.Identity;

public sealed class HttpTenantContext(ICurrentUser currentUser) : ITenantContext
{
    public Guid TenantId => currentUser.IsAuthenticated && currentUser.TenantId is { } tenantId
        ? tenantId
        : throw new InvalidOperationException("An authenticated institutional tenant is required.");
}
