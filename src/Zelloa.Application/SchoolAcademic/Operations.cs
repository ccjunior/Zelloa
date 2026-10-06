using Zelloa.Application.Identity;
using Zelloa.Domain.Guardians;
using Zelloa.Domain.Schools;
using Zelloa.Domain.Tenants;

namespace Zelloa.Application.SchoolAcademic;

public sealed class SchoolOperations(ISchoolAcademicStore store)
{
    public async Task<SchoolResponse> CreateAsync(
        string name,
        string tradeName,
        string? identifier,
        string? contactEmail,
        string? contactPhone,
        string timeZone,
        CancellationToken cancellationToken)
    {
        var tenant = new Tenant(Guid.NewGuid(), name);
        var school = new School(Guid.NewGuid(), tenant.Id, name, tradeName, identifier, contactEmail, contactPhone, timeZone);
        store.Add(tenant);
        store.Add(school);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(school);
    }

    public async Task<SchoolResponse?> GetAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var school = await store.GetSchoolForTenantAsync(tenantId, cancellationToken);
        return school is null ? null : ToResponse(school);
    }

    public async Task<SchoolResponse?> UpdateAsync(
        Guid tenantId,
        string name,
        string tradeName,
        string? identifier,
        string? contactEmail,
        string? contactPhone,
        string timeZone,
        CancellationToken cancellationToken)
    {
        var school = await store.GetSchoolForTenantAsync(tenantId, cancellationToken);
        if (school is null) return null;
        school.Update(name, tradeName, identifier, contactEmail, contactPhone, timeZone);
        (await store.GetTenantIgnoringTenantFilterAsync(tenantId, cancellationToken))?.Rename(name);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(school);
    }

    private static SchoolResponse ToResponse(School school) =>
        new(school.Id, school.TenantId, school.Name, school.TradeName, school.Identifier,
            school.ContactEmail, school.ContactPhone, school.TimeZone, school.IsActive);
}

public sealed class ClassroomOperations(ISchoolAcademicStore store, ITenantContext tenantContext)
{
    public async Task<ClassroomResponse> CreateAsync(
        string name, string level, string shift, int academicYear, CancellationToken cancellationToken)
    {
        var classroom = new Classroom(Guid.NewGuid(), tenantContext.TenantId, name, level, shift, academicYear);
        store.Add(classroom);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(classroom);
    }

    public async Task<PagedResponse<ClassroomResponse>> ListAsync(
        int? academicYear, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await store.CountClassroomsAsync(academicYear, active, search, cancellationToken);
        var items = await store.GetClassroomsAsync(academicYear, active, search, page, pageSize, cancellationToken);
        return new(items.Select(ToResponse).ToArray(), page, pageSize, total);
    }

    public async Task<ClassroomResponse?> GetAsync(Guid classroomId, CancellationToken cancellationToken)
    {
        var classroom = await store.GetClassroomAsync(classroomId, cancellationToken);
        return classroom is null ? null : ToResponse(classroom);
    }

    public async Task<ClassroomResponse?> UpdateAsync(
        Guid classroomId, string name, string level, string shift, int academicYear, CancellationToken cancellationToken)
    {
        var classroom = await store.GetClassroomAsync(classroomId, cancellationToken);
        if (classroom is null) return null;
        classroom.Update(name, level, shift, academicYear);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(classroom);
    }

    public async Task<ClassroomResponse?> SetStatusAsync(
        Guid classroomId, bool active, CancellationToken cancellationToken)
    {
        var classroom = await store.GetClassroomAsync(classroomId, cancellationToken);
        if (classroom is null) return null;
        classroom.SetActive(active);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(classroom);
    }

    private static ClassroomResponse ToResponse(Classroom classroom) =>
        new(classroom.Id, classroom.Name, classroom.Level, classroom.Shift, classroom.AcademicYear, classroom.IsActive);
}

public sealed class StudentOperations(ISchoolAcademicStore store, ITenantContext tenantContext, ICurrentUser currentUser)
{
    public async Task<StudentResponse?> CreateAsync(
        string name, Guid classroomId, CancellationToken cancellationToken)
    {
        var classroom = await store.GetClassroomAsync(classroomId, cancellationToken);
        if (classroom is null || !classroom.IsActive) return null;
        var student = new Student(Guid.NewGuid(), tenantContext.TenantId, classroomId, name);
        store.Add(student);
        await store.SaveChangesAsync(cancellationToken);
        return new(student.Id, student.Name, classroom.Id, classroom.Name, student.IsActive);
    }

    public async Task<PagedResponse<StudentResponse>> ListAsync(
        Guid? classroomId, bool? active, string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var total = await store.CountStudentsAsync(classroomId, active, search, cancellationToken);
        var students = await store.GetStudentsAsync(classroomId, active, search, page, pageSize, cancellationToken);
        var classrooms = await store.GetClassroomsByIdsAsync(
            students.Select(student => student.ClassroomId).Distinct().ToArray(), cancellationToken);
        var classroomNames = classrooms.ToDictionary(classroom => classroom.Id, classroom => classroom.Name);
        var items = students
            .Where(student => classroomNames.ContainsKey(student.ClassroomId))
            .Select(student => new StudentResponse(
                student.Id, student.Name, student.ClassroomId, classroomNames[student.ClassroomId], student.IsActive))
            .ToArray();
        return new(items, page, pageSize, total);
    }

    public async Task<StudentResponse?> GetAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await store.GetStudentAsync(studentId, cancellationToken);
        if (student is null) return null;
        if (currentUser.Roles.Contains(ZelloaRoles.Guardian)
            && !await store.GuardianStudentExistsAsync(currentUser.UserId!.Value, studentId, cancellationToken))
            return null;
        var classroom = await store.GetClassroomAsync(student.ClassroomId, cancellationToken);
        return classroom is null ? null : new(student.Id, student.Name, classroom.Id, classroom.Name, student.IsActive);
    }

    public async Task<StudentResponse?> UpdateAsync(
        Guid studentId, string name, Guid classroomId, CancellationToken cancellationToken)
    {
        var student = await store.GetStudentAsync(studentId, cancellationToken);
        var classroom = await store.GetClassroomAsync(classroomId, cancellationToken);
        if (student is null || classroom is null || !classroom.IsActive) return null;
        student.Update(name, classroomId);
        await store.SaveChangesAsync(cancellationToken);
        return new(student.Id, student.Name, classroom.Id, classroom.Name, student.IsActive);
    }

    public async Task<StudentResponse?> SetStatusAsync(
        Guid studentId, bool active, CancellationToken cancellationToken)
    {
        var student = await store.GetStudentAsync(studentId, cancellationToken);
        if (student is null) return null;
        student.SetActive(active);
        await store.SaveChangesAsync(cancellationToken);
        var classroom = await store.GetClassroomAsync(student.ClassroomId, cancellationToken);
        return classroom is null ? null : new(student.Id, student.Name, classroom.Id, classroom.Name, student.IsActive);
    }

    public async Task<IReadOnlyList<StudentResponse>> GetForCurrentGuardianAsync(CancellationToken cancellationToken)
    {
        var guardianId = currentUser.UserId!.Value;
        var students = await store.GetGuardianStudentsAsync(guardianId, cancellationToken);
        var classrooms = await store.GetClassroomsByIdsAsync(
            students.Select(student => student.ClassroomId).Distinct().ToArray(), cancellationToken);
        var classroomNames = classrooms.ToDictionary(classroom => classroom.Id, classroom => classroom.Name);
        return students
            .Where(student => classroomNames.ContainsKey(student.ClassroomId))
            .Select(student => new StudentResponse(
                student.Id, student.Name, student.ClassroomId, classroomNames[student.ClassroomId], student.IsActive))
            .ToArray();
    }
}

public sealed class GuardianOperations(
    ISchoolAcademicStore store,
    ITenantContext tenantContext,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<bool> LinkAsync(Guid studentId, Guid guardianId, CancellationToken cancellationToken)
    {
        var student = await store.GetStudentAsync(studentId, cancellationToken);
        var guardian = await store.GetGuardianAsync(guardianId, cancellationToken);
        if (student is null || guardian is null || !student.IsActive) return false;
        if (await store.GuardianStudentExistsAsync(guardianId, studentId, cancellationToken)) return true;

        store.Add(new GuardianStudent(guardianId, studentId, tenantContext.TenantId, timeProvider.GetUtcNow()));
        await store.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnlinkAsync(Guid studentId, Guid guardianId, CancellationToken cancellationToken)
    {
        var link = await store.GetGuardianStudentAsync(guardianId, studentId, cancellationToken);
        if (link is null) return false;
        store.Remove(link);
        await store.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<IReadOnlyList<StudentResponse>> GetCurrentGuardianStudentsAsync(CancellationToken cancellationToken)
    {
        var students = new StudentOperations(store, tenantContext, currentUser);
        return students.GetForCurrentGuardianAsync(cancellationToken);
    }
}
