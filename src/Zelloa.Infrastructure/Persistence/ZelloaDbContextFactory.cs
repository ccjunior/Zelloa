using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Zelloa.Application.Identity;

namespace Zelloa.Infrastructure.Persistence;

public sealed class ZelloaDbContextFactory : IDesignTimeDbContextFactory<ZelloaDbContext>
{
    public ZelloaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ZelloaDbContext>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("ConnectionStrings__ZelloaDatabase")
                ?? "Host=localhost;Port=5432;Database=zelloa;Username=zelloa;Password=zelloa-local")
            .Options;

        return new ZelloaDbContext(options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
    }
}
