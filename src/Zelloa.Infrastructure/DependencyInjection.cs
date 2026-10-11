using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Application.Catalog;
using Zelloa.Infrastructure.Persistence;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Application.Orders;
using Zelloa.Application.Payments;
using Zelloa.Infrastructure.Payments;

namespace Zelloa.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ZelloaDatabase")
            ?? throw new InvalidOperationException("Connection string 'ZelloaDatabase' is missing.");

        services.AddDbContext<ZelloaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICatalogStore, CatalogStore>();
        services.AddScoped<ISchoolAcademicStore, SchoolAcademicStore>();
        services.AddScoped<IOrderStore, OrderStore>();
        services.AddScoped<IPaymentStore, PaymentStore>();
        services.AddScoped<IPaymentGatewayResolver, PaymentGatewayResolver>();
        services.AddScoped<DevelopmentSimulationPaymentGateway>();
        services.AddScoped<AsaasSandboxPaymentGateway>();
        services.AddSingleton(_ =>
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api-sandbox.asaas.com/v3/"),
                Timeout = TimeSpan.FromSeconds(20)
            };
            var apiKey = configuration["Payments:Asaas:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
                client.DefaultRequestHeaders.Add("access_token", apiKey);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Zelloa/1.0");
            return client;
        });

        return services;
    }
}
