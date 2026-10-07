using Zelloa.Infrastructure;
using Zelloa.Api.Identity;
using Zelloa.Api.SchoolAcademic;
using Zelloa.Api.Catalog;
using Zelloa.Application.Catalog;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Application.Orders;
using Zelloa.Api.Orders;
using Zelloa.Api.Payments;
using Zelloa.Application.Payments;
using Zelloa.Infrastructure.Payments;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddZelloaIdentity(builder.Configuration, builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration);
var paymentSimulationEnabled = builder.Configuration.GetValue<bool>("Payments:SimulationEnabled");
if (paymentSimulationEnabled && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Payment simulation can only be enabled in Development.");
if (paymentSimulationEnabled)
{
    builder.Services.AddScoped<IPaymentGateway, DevelopmentSimulationPaymentGateway>();
    builder.Services.AddScoped<PaymentOperations>();
}
builder.Services.AddScoped<SchoolOperations>();
builder.Services.AddScoped<CategoryOperations>();
builder.Services.AddScoped<ProductOperations>();
builder.Services.AddScoped<ClassroomOperations>();
builder.Services.AddScoped<StudentOperations>();
builder.Services.AddScoped<GuardianOperations>();
builder.Services.AddScoped<IdentityInvitationService>();
builder.Services.AddScoped<OrderOperations>();
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
app.MapCatalogEndpoints();
app.MapOrderEndpoints();
if (paymentSimulationEnabled)
    app.MapDevelopmentPaymentEndpoints();

await app.Services.BootstrapPlatformAdminAsync(app.Configuration);

app.Run();

public partial class Program { }
