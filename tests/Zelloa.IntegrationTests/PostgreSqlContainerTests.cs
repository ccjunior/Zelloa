using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Zelloa.Application.Identity;
using Zelloa.Infrastructure.Persistence;

namespace Zelloa.IntegrationTests;

public sealed class PostgreSqlContainerTests
{
    [Fact]
    public async Task PostgreSql_container_accepts_a_database_connection()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ZelloaDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using var dbContext = new ZelloaDbContext(options, new TestTenantContext());

        Assert.True(await dbContext.Database.CanConnectAsync());
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.NewGuid();
    }
}
