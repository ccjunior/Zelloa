using Zelloa.Infrastructure;
using Zelloa.Api.Identity;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Application.SchoolAcademic;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddZelloaIdentity(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SchoolOperations>();
builder.Services.AddScoped<ClassroomOperations>();
builder.Services.AddScoped<StudentOperations>();
builder.Services.AddScoped<GuardianOperations>();
builder.Services.AddScoped<IdentityInvitationService>();
builder.Services.AddHealthChecks().AddCheck<Zelloa.Api.DatabaseHealthCheck>("postgresql");

var app = builder.Build();

app.UseExceptionHandler();
app.UseRouting();
app.UseCors("ZelloaWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapHealthChecks("/health");
app.MapIdentityEndpoints();
app.MapSchoolAcademicEndpoints();

await app.Services.BootstrapPlatformAdminAsync(app.Configuration);

app.Run();

public partial class Program { }
