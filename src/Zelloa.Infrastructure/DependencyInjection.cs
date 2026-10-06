using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Application.Catalog;
using Zelloa.Infrastructure.Persistence;
using Zelloa.Application.SchoolAcademic;
using Zelloa.Application.Orders;

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

        return services;
    }
}
