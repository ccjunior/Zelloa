namespace Zelloa.Domain.Schools;

public sealed class Classroom
{
    private Classroom() { }

    public Classroom(Guid id, Guid tenantId, string name, string level, string shift, int academicYear)
    {
        if (id == Guid.Empty) throw new ArgumentException("Classroom ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(level);
        ArgumentException.ThrowIfNullOrWhiteSpace(shift);
        if (academicYear is < 2000 or > 2200) throw new ArgumentOutOfRangeException(nameof(academicYear));

        Id = id;
        TenantId = tenantId;
        Name = name.Trim();
        Level = level.Trim();
        Shift = shift.Trim();
        AcademicYear = academicYear;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public string Shift { get; private set; } = string.Empty;
    public int AcademicYear { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(string name, string level, string shift, int academicYear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(level);
        ArgumentException.ThrowIfNullOrWhiteSpace(shift);
        if (academicYear is < 2000 or > 2200) throw new ArgumentOutOfRangeException(nameof(academicYear));
        Name = name.Trim();
        Level = level.Trim();
        Shift = shift.Trim();
        AcademicYear = academicYear;
    }

    public void SetActive(bool active) => IsActive = active;
}
