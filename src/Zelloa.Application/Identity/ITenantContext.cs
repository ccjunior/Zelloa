namespace Zelloa.Application.Identity;

public interface ITenantContext
{
    Guid TenantId { get; }
}
