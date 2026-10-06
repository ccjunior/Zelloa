namespace Zelloa.Domain.Schools;

public sealed class Student
{
    private Student() { }

    public Student(Guid id, Guid tenantId, Guid classroomId, string name)
    {
        if (id == Guid.Empty) throw new ArgumentException("Student ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (classroomId == Guid.Empty) throw new ArgumentException("Classroom ID cannot be empty.", nameof(classroomId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        TenantId = tenantId;
        ClassroomId = classroomId;
        Name = name.Trim();
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ClassroomId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void Update(string name, Guid classroomId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (classroomId == Guid.Empty) throw new ArgumentException("Classroom ID cannot be empty.", nameof(classroomId));
        Name = name.Trim();
        ClassroomId = classroomId;
    }

    public void SetActive(bool active) => IsActive = active;
}
