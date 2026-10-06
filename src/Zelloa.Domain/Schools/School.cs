namespace Zelloa.Domain.Schools;

public sealed class School
{
    private School() { }

    public School(
        Guid id,
        Guid tenantId,
        string name,
        string tradeName,
        string? identifier,
        string? contactEmail,
        string? contactPhone,
        string timeZone)
    {
        if (id == Guid.Empty) throw new ArgumentException("School ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);

        Id = id;
        TenantId = tenantId;
        Name = name.Trim();
        TradeName = tradeName.Trim();
        Identifier = string.IsNullOrWhiteSpace(identifier) ? null : identifier.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        TimeZone = timeZone.Trim();
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string TradeName { get; private set; } = string.Empty;
    public string? Identifier { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void Update(
        string name,
        string tradeName,
        string? identifier,
        string? contactEmail,
        string? contactPhone,
        string timeZone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tradeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZone);
        Name = name.Trim();
        TradeName = tradeName.Trim();
        Identifier = string.IsNullOrWhiteSpace(identifier) ? null : identifier.Trim();
        ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim();
        ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim();
        TimeZone = timeZone.Trim();
    }

    public void SetActive(bool active) => IsActive = active;
}
