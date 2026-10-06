using Microsoft.EntityFrameworkCore;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Domain.Guardians;
using Zelloa.Domain.Invitations;
using Zelloa.Domain.Schools;
using Zelloa.Domain.Tenants;

namespace Zelloa.Infrastructure.Persistence;

public sealed class SchoolAcademicStore(ZelloaDbContext dbContext) : ISchoolAcademicStore
{
    public Task<School?> GetSchoolForTenantAsync(Guid tenantId, CancellationToken cancellationToken) =>
        dbContext.Schools.FirstOrDefaultAsync(school => school.TenantId == tenantId, cancellationToken);

    public Task<School?> GetSchoolIgnoringTenantFilterAsync(Guid schoolId, CancellationToken cancellationToken) =>
        dbContext.Schools.IgnoreQueryFilters().FirstOrDefaultAsync(school => school.Id == schoolId, cancellationToken);

    public Task<Tenant?> GetTenantIgnoringTenantFilterAsync(Guid tenantId, CancellationToken cancellationToken) =>
        dbContext.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);

    public Task<Classroom?> GetClassroomAsync(Guid classroomId, CancellationToken cancellationToken) =>
        dbContext.Classrooms.FirstOrDefaultAsync(classroom => classroom.Id == classroomId, cancellationToken);

    public async Task<IReadOnlyList<Classroom>> GetClassroomsAsync(
        int? academicYear, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = FilterClassrooms(academicYear, active, search);
        return await query.OrderBy(classroom => classroom.AcademicYear)
            .ThenBy(classroom => classroom.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Classroom>> GetClassroomsByIdsAsync(
        IReadOnlyCollection<Guid> classroomIds, CancellationToken cancellationToken)
    {
        if (classroomIds.Count == 0) return [];
        return await dbContext.Classrooms
            .Where(classroom => classroomIds.Contains(classroom.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountClassroomsAsync(
        int? academicYear, bool? active, string? search, CancellationToken cancellationToken) =>
        FilterClassrooms(academicYear, active, search).CountAsync(cancellationToken);

    public Task<Student?> GetStudentAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Students.FirstOrDefaultAsync(student => student.Id == studentId, cancellationToken);

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(
        Guid? classroomId, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = FilterStudents(classroomId, active, search);
        return await query.OrderBy(student => student.Name)
            .ThenBy(student => student.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountStudentsAsync(
        Guid? classroomId, bool? active, string? search, CancellationToken cancellationToken) =>
        FilterStudents(classroomId, active, search).CountAsync(cancellationToken);

    public Task<Guardian?> GetGuardianAsync(Guid guardianId, CancellationToken cancellationToken) =>
        dbContext.Guardians.FirstOrDefaultAsync(guardian => guardian.Id == guardianId, cancellationToken);

    public Task<GuardianStudent?> GetGuardianStudentAsync(
        Guid guardianId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.GuardianStudents.FirstOrDefaultAsync(
            link => link.GuardianId == guardianId && link.StudentId == studentId,
            cancellationToken);

    public async Task<IReadOnlyList<Student>> GetGuardianStudentsAsync(Guid guardianId, CancellationToken cancellationToken)
    {
        var studentIds = dbContext.GuardianStudents
            .Where(link => link.GuardianId == guardianId)
            .Select(link => link.StudentId);
        return await dbContext.Students
            .Where(student => studentIds.Contains(student.Id) && student.IsActive)
            .OrderBy(student => student.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> GuardianStudentExistsAsync(
        Guid guardianId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.GuardianStudents.AnyAsync(
            link => link.GuardianId == guardianId && link.StudentId == studentId,
            cancellationToken);

    public void Add(Tenant tenant) => dbContext.Tenants.Add(tenant);
    public void Add(School school) => dbContext.Schools.Add(school);
    public void Add(Classroom classroom) => dbContext.Classrooms.Add(classroom);
    public void Add(Student student) => dbContext.Students.Add(student);
    public void Add(Guardian guardian) => dbContext.Guardians.Add(guardian);
    public void Add(GuardianStudent link) => dbContext.GuardianStudents.Add(link);
    public void Add(AccountInvitation invitation) => dbContext.AccountInvitations.Add(invitation);
    public void Remove(GuardianStudent link) => dbContext.GuardianStudents.Remove(link);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _ = await dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Classroom> FilterClassrooms(int? academicYear, bool? active, string? search)
    {
        var query = dbContext.Classrooms.AsQueryable();
        if (academicYear is not null) query = query.Where(classroom => classroom.AcademicYear == academicYear);
        if (active is not null) query = query.Where(classroom => classroom.IsActive == active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(classroom => EF.Functions.ILike(classroom.Name, $"%{term}%"));
        }
        return query;
    }

    private IQueryable<Student> FilterStudents(Guid? classroomId, bool? active, string? search)
    {
        var query = dbContext.Students.AsQueryable();
        if (classroomId is not null) query = query.Where(student => student.ClassroomId == classroomId);
        if (active is not null) query = query.Where(student => student.IsActive == active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(student => EF.Functions.ILike(student.Name, $"%{term}%"));
        }
        return query;
    }
}
