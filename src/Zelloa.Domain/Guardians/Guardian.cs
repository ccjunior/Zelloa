namespace Zelloa.Domain.Guardians;

public sealed class Guardian
{
    private Guardian() { }

    public Guardian(Guid id, Guid tenantId, string displayName)
    {
        if (id == Guid.Empty) throw new ArgumentException("Guardian ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        Id = id;
        TenantId = tenantId;
        DisplayName = displayName.Trim();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
}
