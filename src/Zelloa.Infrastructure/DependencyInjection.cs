using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelloa.Infrastructure.Persistence;
using Zelloa.Application.SchoolAcademic;

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
        services.AddScoped<ISchoolAcademicStore, SchoolAcademicStore>();

        return services;
    }
}
