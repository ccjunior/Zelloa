using Zelloa.Domain.Guardians;
using Zelloa.Domain.Invitations;
using Zelloa.Domain.Schools;
using Zelloa.Domain.Tenants;

namespace Zelloa.Application.SchoolAcademic;

public interface ISchoolAcademicStore
{
    Task<School?> GetSchoolForTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<School?> GetSchoolIgnoringTenantFilterAsync(Guid schoolId, CancellationToken cancellationToken);
    Task<Tenant?> GetTenantIgnoringTenantFilterAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<Classroom?> GetClassroomAsync(Guid classroomId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Classroom>> GetClassroomsAsync(
        int? academicYear, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<Classroom>> GetClassroomsByIdsAsync(
        IReadOnlyCollection<Guid> classroomIds, CancellationToken cancellationToken);
    Task<int> CountClassroomsAsync(int? academicYear, bool? active, string? search, CancellationToken cancellationToken);
    Task<Student?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Student>> GetStudentsAsync(
        Guid? classroomId, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> CountStudentsAsync(Guid? classroomId, bool? active, string? search, CancellationToken cancellationToken);
    Task<Guardian?> GetGuardianAsync(Guid guardianId, CancellationToken cancellationToken);
    Task<GuardianStudent?> GetGuardianStudentAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Student>> GetGuardianStudentsAsync(Guid guardianId, CancellationToken cancellationToken);
    Task<bool> GuardianStudentExistsAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken);
    void Add(Tenant tenant);
    void Add(School school);
    void Add(Classroom classroom);
    void Add(Student student);
    void Add(Guardian guardian);
    void Add(GuardianStudent link);
    void Add(AccountInvitation invitation);
    void Remove(GuardianStudent link);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record SchoolResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string TradeName,
    string? Identifier,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone,
    bool IsActive);
public sealed record ClassroomResponse(Guid Id, string Name, string Level, string Shift, int AcademicYear, bool IsActive);
public sealed record StudentResponse(Guid Id, string Name, Guid ClassroomId, string ClassroomName, bool IsActive);
public sealed record GuardianResponse(Guid Id, string DisplayName, string Email, bool IsActivated);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
