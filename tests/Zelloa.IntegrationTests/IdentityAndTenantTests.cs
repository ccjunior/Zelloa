using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Zelloa.Api.Identity;
using Zelloa.Application.Identity;
using Zelloa.Domain.Tenants;
using Zelloa.Domain.Schools;
using Zelloa.Domain.Guardians;
using Zelloa.Infrastructure.Identity;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.IntegrationTests;

public sealed class IdentityAndTenantTests(IdentityTenantFixture fixture) : IClassFixture<IdentityTenantFixture>
{
    private const string Password = "Zelloa-Test!Password42";
    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StudentAId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid StudentBId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    private readonly IdentityTenantFixture _fixture = fixture;

    [Fact]
    public async Task Anonymous_user_cannot_read_current_user()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Cors_allows_configured_web_origin_with_credentials_only()
    {
        var configuration = _fixture.Factory.Services.GetRequiredService<IConfiguration>();
        Assert.Equal("http://localhost:4200", configuration["Cors:AllowedOrigins:0"]);
        var policy = _fixture.Factory.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<CorsOptions>>()
            .Value.GetPolicy("ZelloaWeb");
        Assert.NotNull(policy);
        Assert.Contains("http://localhost:4200", policy.Origins);

        using var client = _fixture.CreateClient();
        using var allowedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/csrf");
        allowedRequest.Headers.Add("Origin", "http://localhost:4200");

        var allowedResponse = await client.SendAsync(allowedRequest);

        Assert.True(
            allowedResponse.Headers.TryGetValues("Access-Control-Allow-Origin", out var accessControlOrigins),
            $"Expected CORS response header. Actual headers: {string.Join(";", allowedResponse.Headers.SelectMany(header => header.Value.Select(value => $"{header.Key}={value}")))}");
        Assert.Equal("http://localhost:4200", accessControlOrigins!.Single());
        Assert.Equal("true", allowedResponse.Headers.GetValues("Access-Control-Allow-Credentials").Single());

        using var rejectedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/csrf");
        rejectedRequest.Headers.Add("Origin", "https://untrusted.example");

        var rejectedResponse = await client.SendAsync(rejectedRequest);

        Assert.False(rejectedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Login_requires_antiforgery_token_and_establishes_cookie_session()
    {
        using var client = _fixture.CreateClient();

        var missingTokenResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "responsavel-a@zelloa.test", password = Password });
        Assert.Equal(HttpStatusCode.BadRequest, missingTokenResponse.StatusCode);

        var antiforgeryToken = await GetAntiforgeryTokenAsync(client);
        var loginResponse = await PostWithAntiforgeryAsync(
            client,
            "/api/auth/login",
            antiforgeryToken,
            new { email = "responsavel-a@zelloa.test", password = Password });

        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.Equal(true, loginResponse.Headers.CacheControl?.NoStore);
        Assert.Contains("Zelloa.Session", string.Join(";", loginResponse.Headers.GetValues("Set-Cookie")));
        var sessionCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .Single(value => value.StartsWith("Zelloa.Session=", StringComparison.Ordinal));
        Assert.Contains("httponly", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", sessionCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", sessionCookie, StringComparison.OrdinalIgnoreCase);

        var currentUser = await client.GetFromJsonAsync<IdentityEndpoints.CurrentUserResponse>("/api/me");
        Assert.NotNull(currentUser);
        Assert.Equal(_fixture.TenantAUserId, currentUser.UserId);
        Assert.Equal(TenantAId, currentUser.TenantId);
        Assert.Contains("Guardian", currentUser.Roles);

        var authenticatedAntiforgeryToken = await GetAntiforgeryTokenAsync(client);
        var logoutResponse = await PostWithAntiforgeryAsync(
            client,
            "/api/auth/logout",
            authenticatedAntiforgeryToken,
            body: (object?)null);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/me")).StatusCode);
    }

    [Fact]
    public async Task User_without_required_role_is_denied_by_authorization_policy()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ZelloaUser>>();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var user = await userManager.FindByEmailAsync("responsavel-a@zelloa.test");
        Assert.NotNull(user);

        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<ZelloaUser>>();
        var principal = await signInManager.CreateUserPrincipalAsync(user);

        Assert.True((await authorization.AuthorizeAsync(principal, null, "CanCreateOrder")).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(principal, null, "CanManageSchool")).Succeeded);
    }

    [Fact]
    public async Task Tenant_role_without_tenant_assignment_cannot_sign_in()
    {
        await using (var scope = _fixture.Factory.Services.CreateAsyncScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ZelloaUser>>();
            var user = new ZelloaUser
            {
                UserName = "misconfigured@zelloa.test",
                Email = "misconfigured@zelloa.test",
                DisplayName = "Conta sem tenant",
                TenantId = null
            };

            var createResult = await userManager.CreateAsync(user, Password);
            Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));
            var roleResult = await userManager.AddToRoleAsync(user, "Guardian");
            Assert.True(roleResult.Succeeded, string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }

        using var client = _fixture.CreateClient();
        var token = await GetAntiforgeryTokenAsync(client);
        var response = await PostWithAntiforgeryAsync(
            client,
            "/api/auth/login",
            token,
            new { email = "misconfigured@zelloa.test", password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tenant_filter_and_authenticated_context_ignore_client_tenant_values()
    {
        var options = new DbContextOptionsBuilder<ZelloaDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        await using (var tenantADb = new ZelloaDbContext(options, new FixedTenantContext(TenantAId)))
        {
            var visibleTenantIds = await tenantADb.Tenants.Select(tenant => tenant.Id).ToListAsync();
            Assert.Equal(new[] { TenantAId }, visibleTenantIds);
        }

        using var client = _fixture.CreateClient();
        var token = await GetAntiforgeryTokenAsync(client);
        var login = await PostWithAntiforgeryAsync(
            client,
            "/api/auth/login",
            token,
            new { email = "responsavel-a@zelloa.test", password = Password });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/me?tenantId={TenantBId}");
        request.Headers.Add("X-Tenant-Id", TenantBId.ToString());

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var currentUser = await response.Content.ReadFromJsonAsync<IdentityEndpoints.CurrentUserResponse>();

        Assert.NotNull(currentUser);
        Assert.Equal(TenantAId, currentUser.TenantId);
        Assert.DoesNotContain(TenantBId, await GetCurrentTenantIdsForTenantAAsync(options));
    }

    [Fact]
    public async Task Tenant_can_have_multiple_institutional_accounts()
    {
        await using var scope = _fixture.Factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ZelloaUser>>();
        var anotherAccount = new ZelloaUser
        {
            UserName = "outro-responsavel-a@zelloa.test",
            Email = "outro-responsavel-a@zelloa.test",
            DisplayName = "Outro responsável A",
            TenantId = TenantAId
        };

        var result = await userManager.CreateAsync(anotherAccount, Password);

        Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(error => error.Description)));
    }

    [Fact]
    public async Task Platform_admin_bootstrap_creates_global_account_without_tenant()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IdentityBootstrap:PlatformAdmin:Email"] = "platform-admin@zelloa.test",
                ["IdentityBootstrap:PlatformAdmin:Password"] = Password,
                ["IdentityBootstrap:PlatformAdmin:DisplayName"] = "Zelloa Platform Admin"
            })
            .Build();

        await _fixture.Factory.Services.BootstrapPlatformAdminAsync(configuration);

        using var client = _fixture.CreateClient();
        var token = await GetAntiforgeryTokenAsync(client);
        var login = await PostWithAntiforgeryAsync(
            client,
            "/api/auth/login",
            token,
            new { email = "platform-admin@zelloa.test", password = Password });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        var currentUser = await client.GetFromJsonAsync<IdentityEndpoints.CurrentUserResponse>("/api/me");
        Assert.NotNull(currentUser);
        Assert.Null(currentUser.TenantId);
        Assert.Contains("PlatformAdmin", currentUser.Roles);
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetFromJsonAsync<CsrfTokenResponse>("/api/auth/csrf");
        Assert.NotNull(response);
        return response.RequestToken;
    }

    private static async Task<HttpResponseMessage> PostWithAntiforgeryAsync(
        HttpClient client,
        string path,
        string token,
        object? body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("X-XSRF-TOKEN", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request);
    }

    private static async Task<IReadOnlyList<Guid>> GetCurrentTenantIdsForTenantAAsync(
        DbContextOptions<ZelloaDbContext> options)
    {
        await using var dbContext = new ZelloaDbContext(options, new FixedTenantContext(TenantAId));
        return await dbContext.Tenants.Select(tenant => tenant.Id).ToListAsync();
    }

    private sealed class FixedTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId { get; } = tenantId;
    }

}

public sealed class IdentityTenantFixture : IAsyncLifetime
{
    private const string Password = "Zelloa-Test!Password42";
    private static readonly Guid TenantAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid StudentAId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid StudentBId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public string ConnectionString => _postgres.GetConnectionString();

    public HttpClient CreateClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    public Guid TenantAUserId { get; private set; }
    public Guid TenantAAdminUserId { get; private set; }
    public Guid TenantBUserId { get; private set; }
    public Guid TenantAStudentId => StudentAId;
    public Guid TenantBStudentId => StudentBId;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        Factory = new ZelloaApiFactory(_postgres.GetConnectionString());
        _ = Factory.CreateClient();

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
            await dbContext.Database.MigrateAsync();

            dbContext.Tenants.AddRange(
                new Tenant(TenantAId, "Escola A"),
                new Tenant(TenantBId, "Escola B"));
            dbContext.Schools.AddRange(
                new School(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"), TenantAId, "Escola A", "Escola A", null, null, null, "America/Bahia"),
                new School(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2"), TenantBId, "Escola B", "Escola B", null, null, null, "America/Bahia"));
            dbContext.Classrooms.AddRange(
                new Classroom(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaac1"), TenantAId, "1º Ano A", "Fundamental I", "Matutino", 2026),
                new Classroom(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbc2"), TenantBId, "2º Ano B", "Fundamental I", "Vespertino", 2026));
            dbContext.Students.AddRange(
                new Student(StudentAId, TenantAId, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaac1"), "Aluno A"),
                new Student(StudentBId, TenantBId, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbc2"), "Aluno B"));
            await dbContext.SaveChangesAsync();
        }

        await CreateRoleAndUserAsync("Guardian", "responsavel-a@zelloa.test", "Responsável A", TenantAId);
        await CreateRoleAndUserAsync("Guardian", "responsavel-b@zelloa.test", "Responsável B", TenantBId);
        await CreateRoleAndUserAsync("SchoolAdmin", "admin-a@zelloa.test", "Admin Escola A", TenantAId);
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        await _postgres.DisposeAsync();
    }

    private async Task CreateRoleAndUserAsync(
        string roleName,
        string email,
        string displayName,
        Guid tenantId)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ZelloaUser>>();

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            Assert.True(roleResult.Succeeded, string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }

        var user = new ZelloaUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            TenantId = tenantId,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, Password);
        Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

        var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
        Assert.True(addRoleResult.Succeeded, string.Join("; ", addRoleResult.Errors.Select(error => error.Description)));

        if (roleName == ZelloaRoles.Guardian)
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ZelloaDbContext>();
            dbContext.Guardians.Add(new Guardian(user.Id, tenantId, displayName));
            await dbContext.SaveChangesAsync();
        }

        if (tenantId == TenantAId)
        {
            if (roleName == ZelloaRoles.Guardian) TenantAUserId = user.Id;
            if (roleName == ZelloaRoles.SchoolAdmin) TenantAAdminUserId = user.Id;
        }
        else if (roleName == ZelloaRoles.Guardian) TenantBUserId = user.Id;
    }

    private sealed class ZelloaApiFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(Environments.Production);
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ZelloaDbContext>>();
                services.RemoveAll<ZelloaDbContext>();
                services.AddDbContext<ZelloaDbContext>(options => options.UseNpgsql(connectionString));
            });
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Cors:AllowedOrigins:0"] = "http://localhost:4200",
                    ["InvitationUrls:Family"] = "https://family.zelloa.test/activate-invitation",
                    ["InvitationUrls:School"] = "https://school.zelloa.test/activate-invitation"
                }));
        }
    }
}
