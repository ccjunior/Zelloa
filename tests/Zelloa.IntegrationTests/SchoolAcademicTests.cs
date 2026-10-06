using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Api.Identity;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Infrastructure.Identity;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.IntegrationTests;

public sealed class SchoolAcademicTests(IdentityTenantFixture fixture) : IClassFixture<IdentityTenantFixture>
{
    private const string Password = "Zelloa-Test!Password42";
    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid TenantAClassroomId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaac1");
    private static readonly Guid TenantBClassroomId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbc2");

    private readonly IdentityTenantFixture _fixture = fixture;

    [Fact]
    public async Task Platform_admin_creates_school_invites_admin_and_school_admin_creates_student()
    {
        await BootstrapPlatformAdminAsync();
        using var client = _fixture.CreateClient();
        await LoginAsync(client, "platform-admin@zelloa.test", Password);

        using var createSchool = await PostAsync(client, "/api/schools", new
        {
            name = "Escola de Teste",
            tradeName = "Escola Teste",
            identifier = "school-test-001",
            contactEmail = "secretaria@school.test",
            contactPhone = "+5571999990000",
            timeZone = "America/Bahia"
        });
        Assert.Equal(HttpStatusCode.Created, createSchool.StatusCode);
        var school = await createSchool.Content.ReadFromJsonAsync<SchoolResponse>();
        Assert.NotNull(school);

        using var adminInvitationResponse = await PostAsync(
            client,
            $"/api/schools/{school.Id}/administrator-invitation",
            new { displayName = "Admin de Teste", email = "admin-phase2@zelloa.test" });
        Assert.Equal(HttpStatusCode.Created, adminInvitationResponse.StatusCode);
        Assert.True(adminInvitationResponse.Headers.CacheControl?.NoStore);
        var administratorInvitation = await adminInvitationResponse.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.NotNull(administratorInvitation);
        Assert.InRange(administratorInvitation.ExpiresAt, DateTimeOffset.UtcNow.AddHours(23), DateTimeOffset.UtcNow.AddHours(25));

        var adminToken = GetQueryValue(administratorInvitation.ActivationUrl, "token");
        var adminEmail = GetQueryValue(administratorInvitation.ActivationUrl, "email");
        Assert.Equal("admin-phase2@zelloa.test", adminEmail);
        Assert.NotEqual(adminToken, await GetStoredTokenHashAsync(administratorInvitation.UserId));

        Assert.Equal(HttpStatusCode.BadRequest, (await PostAsync(
            client,
            "/api/auth/activate",
            new { email = adminEmail, token = adminToken, password = Password },
            includeAntiforgery: false)).StatusCode);

        using var activation = await PostAsync(client, "/api/auth/activate", new
        {
            email = adminEmail,
            token = adminToken,
            password = Password
        });
        Assert.Equal(HttpStatusCode.NoContent, activation.StatusCode);

        using var adminClient = _fixture.CreateClient();
        await LoginAsync(adminClient, adminEmail, Password);
        using var schoolResponse = await adminClient.GetAsync("/api/school");
        Assert.Equal(HttpStatusCode.OK, schoolResponse.StatusCode);
        Assert.Equal(school.Id, (await schoolResponse.Content.ReadFromJsonAsync<SchoolResponse>())!.Id);

        using var classroomResponse = await PostAsync(adminClient, "/api/classrooms", new
        {
            name = "6º Ano B",
            level = "Fundamental II",
            shift = "Matutino",
            academicYear = 2026
        });
        Assert.Equal(HttpStatusCode.Created, classroomResponse.StatusCode);
        var classroom = await classroomResponse.Content.ReadFromJsonAsync<ClassroomResponse>();
        Assert.NotNull(classroom);

        using var studentResponse = await PostAsync(adminClient, "/api/students", new
        {
            name = "Estudante de Teste",
            classroomId = classroom.Id
        });
        Assert.Equal(HttpStatusCode.Created, studentResponse.StatusCode);
        var student = await studentResponse.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);
        Assert.Equal(classroom.Id, student.ClassroomId);
    }

    [Fact]
    public async Task Guardian_invitation_is_pending_until_activation_and_cannot_be_reused()
    {
        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test", Password);
        var email = $"guardian-{Guid.NewGuid():N}@zelloa.test";

        using var invitationResponse = await PostAsync(schoolAdmin, "/api/guardians", new
        {
            displayName = "Responsável Convidado",
            email
        });

        Assert.Equal(HttpStatusCode.Created, invitationResponse.StatusCode);
        Assert.True(invitationResponse.Headers.CacheControl?.NoStore);
        Assert.Equal("no-referrer", invitationResponse.Headers.GetValues("Referrer-Policy").Single());
        var invitation = await invitationResponse.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.NotNull(invitation);
        var token = GetQueryValue(invitation.ActivationUrl, "token");
        Assert.NotEqual(token, await GetStoredTokenHashAsync(invitation.UserId));

        using var unconfirmedClient = _fixture.CreateClient();
        using var preActivationLogin = await PostAsync(unconfirmedClient, "/api/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, preActivationLogin.StatusCode);

        using var activation = await PostAsync(unconfirmedClient, "/api/auth/activate", new
        {
            email,
            token,
            password = Password
        });
        Assert.Equal(HttpStatusCode.NoContent, activation.StatusCode);

        using var activateAgain = await PostAsync(unconfirmedClient, "/api/auth/activate", new
        {
            email,
            token,
            password = Password
        });
        Assert.Equal(HttpStatusCode.Unauthorized, activateAgain.StatusCode);

        await LoginAsync(unconfirmedClient, email, Password);
        var guardianStudents = await unconfirmedClient.GetFromJsonAsync<StudentResponse[]>("/api/me/students");
        Assert.NotNull(guardianStudents);
        Assert.Empty(guardianStudents);
    }

    [Fact]
    public async Task Expired_invitation_cannot_activate_an_account()
    {
        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test", Password);
        var email = $"expired-{Guid.NewGuid():N}@zelloa.test";
        using var response = await PostAsync(schoolAdmin, "/api/guardians", new
        {
            displayName = "Convite Expirado",
            email
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var invitation = await response.Content.ReadFromJsonAsync<InvitationResponse>();
        Assert.NotNull(invitation);
        var token = GetQueryValue(invitation.ActivationUrl, "token");
        var tokenHash = await GetStoredTokenHashAsync(invitation.UserId);

        await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
            await dbContext.AccountInvitations
                .Where(item => item.UserId == invitation.UserId)
                .ExecuteUpdateAsync(updates => updates.SetProperty(item => item.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)));
        }

        using var client = _fixture.CreateClient();
        using var activation = await PostAsync(client, "/api/auth/activate", new { email, token, password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, activation.StatusCode);
        Assert.NotEmpty(tokenHash);
    }

    [Fact]
    public async Task Tenant_and_guardian_ownership_filters_hide_students_outside_allowed_scope()
    {
        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test", Password);

        using var outOfTenantClassroomStudent = await PostAsync(schoolAdmin, "/api/students", new
        {
            name = "Não deve ser criado",
            classroomId = TenantBClassroomId
        });
        Assert.Equal(HttpStatusCode.NotFound, outOfTenantClassroomStudent.StatusCode);

        using var outOfTenantStudent = await schoolAdmin.GetAsync($"/api/students/{_fixture.TenantBStudentId}");
        Assert.Equal(HttpStatusCode.NotFound, outOfTenantStudent.StatusCode);
        using var outOfTenantList = await schoolAdmin.GetAsync($"/api/students?classroomId={TenantBClassroomId}");
        Assert.Equal(HttpStatusCode.OK, outOfTenantList.StatusCode);
        Assert.Empty((await outOfTenantList.Content.ReadFromJsonAsync<PagedResponse<StudentResponse>>())!.Items);

        using var guardian = _fixture.CreateClient();
        await LoginAsync(guardian, "responsavel-a@zelloa.test", Password);
        using var unlinkedStudent = await guardian.GetAsync($"/api/students/{_fixture.TenantAStudentId}");
        Assert.Equal(HttpStatusCode.NotFound, unlinkedStudent.StatusCode);

        using var linkOwnTenant = await PostAsync(
            schoolAdmin,
            $"/api/students/{_fixture.TenantAStudentId}/guardians/{_fixture.TenantAUserId}",
            body: null);
        Assert.Equal(HttpStatusCode.NoContent, linkOwnTenant.StatusCode);

        using var relogin = await LoginAsync(guardian, "responsavel-a@zelloa.test", Password);
        Assert.Equal(HttpStatusCode.NoContent, relogin.StatusCode);
        using var linkedStudent = await guardian.GetAsync($"/api/students/{_fixture.TenantAStudentId}");
        Assert.Equal(HttpStatusCode.OK, linkedStudent.StatusCode);
        using var otherTenantStudent = await guardian.GetAsync($"/api/students/{_fixture.TenantBStudentId}");
        Assert.Equal(HttpStatusCode.NotFound, otherTenantStudent.StatusCode);
    }

    [Fact]
    public async Task Guardian_cannot_create_academic_records_and_cannot_link_cross_tenant_students()
    {
        using var guardian = _fixture.CreateClient();
        await LoginAsync(guardian, "responsavel-a@zelloa.test", Password);

        using var deniedClassroom = await PostAsync(guardian, "/api/classrooms", new
        {
            name = "Forbidden",
            level = "X",
            shift = "Manhã",
            academicYear = 2026
        });
        Assert.Equal(HttpStatusCode.Forbidden, deniedClassroom.StatusCode);

        using var schoolAdmin = _fixture.CreateClient();
        await LoginAsync(schoolAdmin, "admin-a@zelloa.test", Password);
        using var crossTenantLink = await PostAsync(
            schoolAdmin,
            $"/api/students/{_fixture.TenantBStudentId}/guardians/{_fixture.TenantAUserId}",
            body: null);
        Assert.Equal(HttpStatusCode.NotFound, crossTenantLink.StatusCode);
    }

    private async Task BootstrapPlatformAdminAsync()
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IdentityBootstrap:PlatformAdmin:Email"] = "platform-admin@zelloa.test",
                ["IdentityBootstrap:PlatformAdmin:Password"] = Password,
                ["IdentityBootstrap:PlatformAdmin:DisplayName"] = "Platform Admin"
            })
            .Build();
        await _fixture.Factory.Services.BootstrapPlatformAdminAsync(configuration);
    }

    private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        var token = await GetAntiforgeryTokenAsync(client);
        return await PostAsync(client, "/api/auth/login", new { email, password }, token);
    }

    private static async Task<HttpResponseMessage> PostAsync(
        HttpClient client,
        string path,
        object? body,
        string? antiforgeryToken = null,
        bool includeAntiforgery = true)
    {
        var token = includeAntiforgery ? antiforgeryToken ?? await GetAntiforgeryTokenAsync(client) : null;
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (token is not null) request.Headers.Add("X-XSRF-TOKEN", token);
        if (body is not null) request.Content = JsonContent.Create(body);
        return await client.SendAsync(request);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<CsrfTokenResponse>("/api/auth/csrf");
        Assert.NotNull(token);
        return token.RequestToken;
    }

    private async Task<string> GetStoredTokenHashAsync(Guid userId)
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
        return await dbContext.AccountInvitations
            .IgnoreQueryFilters()
            .Where(invitation => invitation.UserId == userId)
            .Select(invitation => invitation.TokenHash)
            .SingleAsync();
    }

    private static string GetQueryValue(string url, string key) =>
        QueryHelpers.ParseQuery(new Uri(url).Query)[key].Single()!;
}
