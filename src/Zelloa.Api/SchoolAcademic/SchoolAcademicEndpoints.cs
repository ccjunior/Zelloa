using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Zelloa.Api.Identity;
using Zelloa.Application.Identity;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Domain.Guardians;
using Zelloa.Infrastructure.Identity;

namespace Zelloa.Api.SchoolAcademic;

public static class SchoolAcademicEndpoints
{
    public static IEndpointRouteBuilder MapSchoolAcademicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api").WithTags("School and Academic");

        api.MapPost("/schools", async (
                CreateSchoolRequest request,
                SchoolOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidSchool(request))
                    return Results.BadRequest();
                var response = await operations.CreateAsync(
                    request.Name, request.TradeName, request.Identifier, request.ContactEmail,
                    request.ContactPhone, request.TimeZone, cancellationToken);
                return Results.Json(response, statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.PlatformAdmin))
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateSchool");

        api.MapPost("/schools/{schoolId:guid}/administrator-invitation", async (
                Guid schoolId,
                CreateInvitationRequest request,
                HttpContext context,
                IdentityInvitationService invitations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidInvitation(request)) return Results.BadRequest();
                var result = await invitations.InviteSchoolAdminAsync(
                    schoolId, request.DisplayName, request.Email, cancellationToken);
                if (result.Status == InvitationStatus.NotFound) return Results.NotFound();
                if (result.Status == InvitationStatus.Conflict) return Results.Conflict();
                if (result.Status == InvitationStatus.Misconfigured) return Results.Problem();
                SetInvitationResponseHeaders(context);
                return Results.Json(
                    new InvitationResponse(result.UserId!.Value, result.ActivationUrl!, result.ExpiresAt!.Value),
                    statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.PlatformAdmin))
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("InviteSchoolAdministrator");

        api.MapGet("/school", async (
                ICurrentUser currentUser,
                SchoolOperations operations,
                CancellationToken cancellationToken) =>
            {
                var response = currentUser.TenantId is { } tenantId
                    ? await operations.GetAsync(tenantId, cancellationToken)
                    : null;
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.SchoolAdmin, ZelloaRoles.CafeteriaOperator))
            .WithName("GetCurrentSchool");

        api.MapPut("/school", async (
                UpdateSchoolRequest request,
                ICurrentUser currentUser,
                SchoolOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidSchool(request))
                    return Results.BadRequest();
                if (currentUser.TenantId is not { } tenantId) return Results.NotFound();
                var response = await operations.UpdateAsync(
                    tenantId, request.Name, request.TradeName, request.Identifier, request.ContactEmail,
                    request.ContactPhone, request.TimeZone, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UpdateCurrentSchool");

        api.MapPost("/classrooms", async (
                CreateClassroomRequest request,
                ClassroomOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidClassroom(request.Name, request.Level, request.Shift, request.AcademicYear))
                    return Results.BadRequest();
                var response = await operations.CreateAsync(
                    request.Name, request.Level, request.Shift, request.AcademicYear, cancellationToken);
                return Results.Json(response, statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateClassroom");

        api.MapGet("/classrooms", async (
                int? academicYear,
                string? status,
                string? search,
                int? page,
                int? pageSize,
                ClassroomOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(status, out var active) || !IsValidPaging(page, pageSize) || !IsValidSearch(search))
                    return Results.BadRequest();
                return Results.Ok(await operations.ListAsync(
                    academicYear, active, search, page ?? 1, pageSize ?? 20, cancellationToken));
            })
            .RequireAuthorization("CanManageSchool")
            .WithName("GetClassrooms");

        api.MapGet("/classrooms/{classroomId:guid}", async (
                Guid classroomId,
                ClassroomOperations operations,
                CancellationToken cancellationToken) =>
            {
                var response = await operations.GetAsync(classroomId, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .WithName("GetClassroom");

        api.MapPut("/classrooms/{classroomId:guid}", async (
                Guid classroomId,
                UpdateClassroomRequest request,
                ClassroomOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidClassroom(request.Name, request.Level, request.Shift, request.AcademicYear))
                    return Results.BadRequest();
                var response = await operations.UpdateAsync(
                    classroomId, request.Name, request.Level, request.Shift, request.AcademicYear, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UpdateClassroom");

        api.MapPatch("/classrooms/{classroomId:guid}/status", async (
                Guid classroomId,
                SetStatusRequest request,
                ClassroomOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(request.Status, out var active)) return Results.BadRequest();
                var response = await operations.SetStatusAsync(classroomId, active!.Value, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetClassroomStatus");

        api.MapPost("/students", async (
                CreateStudentRequest request,
                StudentOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidStudent(request.Name, request.ClassroomId)) return Results.BadRequest();
                var response = await operations.CreateAsync(request.Name, request.ClassroomId, cancellationToken);
                return response is null ? Results.NotFound() : Results.Json(response, statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateStudent");

        api.MapGet("/students", async (
                Guid? classroomId,
                string? status,
                string? search,
                int? page,
                int? pageSize,
                StudentOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(status, out var active) || !IsValidPaging(page, pageSize) || !IsValidSearch(search))
                    return Results.BadRequest();
                return Results.Ok(await operations.ListAsync(
                    classroomId, active, search, page ?? 1, pageSize ?? 20, cancellationToken));
            })
            .RequireAuthorization("CanManageSchool")
            .WithName("GetStudents");

        api.MapGet("/students/{studentId:guid}", async (
                Guid studentId,
                StudentOperations operations,
                CancellationToken cancellationToken) =>
            {
                var response = await operations.GetAsync(studentId, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanViewStudent")
            .WithName("GetStudent");

        api.MapPut("/students/{studentId:guid}", async (
                Guid studentId,
                UpdateStudentRequest request,
                StudentOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidStudent(request.Name, request.ClassroomId)) return Results.BadRequest();
                var response = await operations.UpdateAsync(
                    studentId, request.Name, request.ClassroomId, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UpdateStudent");

        api.MapPatch("/students/{studentId:guid}/status", async (
                Guid studentId,
                SetStatusRequest request,
                StudentOperations operations,
                CancellationToken cancellationToken) =>
            {
                if (!TryParseStatus(request.Status, out var active)) return Results.BadRequest();
                var response = await operations.SetStatusAsync(studentId, active!.Value, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("SetStudentStatus");

        api.MapPost("/guardians", async (
                CreateInvitationRequest request,
                HttpContext context,
                ICurrentUser currentUser,
                IdentityInvitationService invitations,
                CancellationToken cancellationToken) =>
            {
                if (!IsValidInvitation(request) || currentUser.TenantId is not { } tenantId)
                    return Results.BadRequest();
                var result = await invitations.InviteGuardianAsync(
                    tenantId, request.DisplayName, request.Email, cancellationToken);
                if (result.Status == InvitationStatus.Conflict) return Results.Conflict();
                if (result.Status == InvitationStatus.Misconfigured) return Results.Problem();
                SetInvitationResponseHeaders(context);
                return Results.Json(
                    new InvitationResponse(result.UserId!.Value, result.ActivationUrl!, result.ExpiresAt!.Value),
                    statusCode: StatusCodes.Status201Created);
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("CreateGuardian");

        api.MapGet("/guardians/{guardianId:guid}", async (
                Guid guardianId,
                UserManager<ZelloaUser> userManager,
                Zelloa.Infrastructure.Persistence.ZelloaDbContext dbContext,
                CancellationToken cancellationToken) =>
            {
                var guardian = await dbContext.Guardians.FirstOrDefaultAsync(
                    item => item.Id == guardianId, cancellationToken);
                if (guardian is null) return Results.NotFound();
                var user = await userManager.FindByIdAsync(guardianId.ToString());
                return user is null
                    ? Results.NotFound()
                    : Results.Ok(new GuardianResponse(guardian.Id, guardian.DisplayName, user.Email ?? string.Empty, user.EmailConfirmed));
            })
            .RequireAuthorization("CanManageSchool")
            .WithName("GetGuardian");

        api.MapPost("/students/{studentId:guid}/guardians/{guardianId:guid}", async (
                Guid studentId,
                Guid guardianId,
                GuardianOperations operations,
                CancellationToken cancellationToken) =>
            {
                var linked = await operations.LinkAsync(studentId, guardianId, cancellationToken);
                return linked ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("LinkGuardianToStudent");

        api.MapDelete("/students/{studentId:guid}/guardians/{guardianId:guid}", async (
                Guid studentId,
                Guid guardianId,
                GuardianOperations operations,
                CancellationToken cancellationToken) =>
            {
                var unlinked = await operations.UnlinkAsync(studentId, guardianId, cancellationToken);
                return unlinked ? Results.NoContent() : Results.NotFound();
            })
            .RequireAuthorization("CanManageSchool")
            .AddEndpointFilter<AntiforgeryEndpointFilter>()
            .WithName("UnlinkGuardianFromStudent");

        api.MapGet("/me/students", async (
                GuardianOperations operations,
                CancellationToken cancellationToken) =>
                Results.Ok(await operations.GetCurrentGuardianStudentsAsync(cancellationToken)))
            .RequireAuthorization(policy => policy.RequireRole(ZelloaRoles.Guardian))
            .WithName("GetGuardianStudents");

        return endpoints;
    }

    private static bool IsValidSchool(SchoolRequest request) =>
        !string.IsNullOrWhiteSpace(request.Name) && request.Name.Trim().Length <= 200
        && !string.IsNullOrWhiteSpace(request.TradeName) && request.TradeName.Trim().Length <= 200
        && (request.Identifier is null || request.Identifier.Trim().Length <= 64)
        && (request.ContactEmail is null || (request.ContactEmail.Trim().Length <= 256
            && new EmailAddressAttribute().IsValid(request.ContactEmail.Trim())))
        && (request.ContactPhone is null || request.ContactPhone.Trim().Length <= 32)
        && !string.IsNullOrWhiteSpace(request.TimeZone) && request.TimeZone.Trim().Length <= 100
        && TimeZoneInfo.TryFindSystemTimeZoneById(request.TimeZone.Trim(), out _);

    private static bool IsValidClassroom(string name, string level, string shift, int academicYear) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 200
        && !string.IsNullOrWhiteSpace(level) && level.Trim().Length <= 100
        && !string.IsNullOrWhiteSpace(shift) && shift.Trim().Length <= 50
        && academicYear is >= 2000 and <= 2200;

    private static bool IsValidStudent(string name, Guid classroomId) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 200 && classroomId != Guid.Empty;

    private static bool IsValidInvitation(CreateInvitationRequest request) =>
        request.DisplayName is not null && request.Email is not null
        && !string.IsNullOrWhiteSpace(request.DisplayName)
        && request.DisplayName.Trim().Length <= 200
        && request.Email.Trim().Length <= 256
        && new EmailAddressAttribute().IsValid(request.Email.Trim());

    private static bool IsValidSearch(string? search) => search is null || search.Trim().Length <= 100;

    private static bool IsValidPaging(int? page, int? pageSize) =>
        page is null or (>= 1 and <= 1_000_000)
        && pageSize is null or (>= 1 and <= 100);

    private static bool TryParseStatus(string? status, out bool? active)
    {
        active = null;
        if (status is null) return true;
        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            active = true;
            return true;
        }
        if (status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
        {
            active = false;
            return true;
        }
        return false;
    }

    private static void SetInvitationResponseHeaders(HttpContext context)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
    }
}

public sealed class AntiforgeryEndpointFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest();
        }

        return await next(context);
    }
}

public abstract record SchoolRequest(
    string Name,
    string TradeName,
    string? Identifier,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone);
public sealed record CreateSchoolRequest(
    string Name,
    string TradeName,
    string? Identifier,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone) : SchoolRequest(Name, TradeName, Identifier, ContactEmail, ContactPhone, TimeZone);
public sealed record UpdateSchoolRequest(
    string Name,
    string TradeName,
    string? Identifier,
    string? ContactEmail,
    string? ContactPhone,
    string TimeZone) : SchoolRequest(Name, TradeName, Identifier, ContactEmail, ContactPhone, TimeZone);
public sealed record CreateClassroomRequest(string Name, string Level, string Shift, int AcademicYear);
public sealed record UpdateClassroomRequest(string Name, string Level, string Shift, int AcademicYear);
public sealed record CreateStudentRequest(string Name, Guid ClassroomId);
public sealed record UpdateStudentRequest(string Name, Guid ClassroomId);
public sealed record SetStatusRequest(string Status);
public sealed record CreateInvitationRequest(string DisplayName, string Email);
public sealed record InvitationResponse(Guid UserId, string ActivationUrl, DateTimeOffset ExpiresAt);
