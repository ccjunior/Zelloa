namespace Zelloa.Domain.Guardians;

public sealed class GuardianStudent
{
    private GuardianStudent() { }

    public GuardianStudent(Guid guardianId, Guid studentId, Guid tenantId, DateTimeOffset linkedAt)
    {
        if (guardianId == Guid.Empty) throw new ArgumentException("Guardian ID cannot be empty.", nameof(guardianId));
        if (studentId == Guid.Empty) throw new ArgumentException("Student ID cannot be empty.", nameof(studentId));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        GuardianId = guardianId;
        StudentId = studentId;
        TenantId = tenantId;
        LinkedAt = linkedAt;
    }

    public Guid GuardianId { get; private set; }
    public Guid StudentId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTimeOffset LinkedAt { get; private set; }
}
