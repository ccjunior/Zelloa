namespace Zelloa.Domain.Catalog;

public sealed class Category
{
    private Category() { }

    public Category(Guid id, Guid tenantId, string name)
    {
        if (id == Guid.Empty) throw new ArgumentException("Category ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 100) throw new ArgumentOutOfRangeException(nameof(name));

        Id = id;
        TenantId = tenantId;
        Name = name.Trim();
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 100) throw new ArgumentOutOfRangeException(nameof(name));
        Name = name.Trim();
    }

    public void SetStatus(bool active) => IsActive = active;
}
