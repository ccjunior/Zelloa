using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Zelloa.Application.Identity;
using Zelloa.Domain.Tenants;
using Zelloa.Infrastructure.Identity;

namespace Zelloa.Infrastructure.Persistence;

public sealed class ZelloaDbContext(
    DbContextOptions<ZelloaDbContext> options,
    ITenantContext tenantContext)
    : IdentityDbContext<ZelloaUser, IdentityRole<Guid>, Guid>(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public Guid CurrentTenantId => _tenantContext.TenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(tenant => tenant.Id);
            entity.Property(tenant => tenant.Name).HasMaxLength(200).IsRequired();
            entity.HasQueryFilter(tenant => tenant.Id == CurrentTenantId);
        });

        modelBuilder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                Name = "PlatformAdmin",
                NormalizedName = "PLATFORMADMIN",
                ConcurrencyStamp = "role-platform-admin-v1"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"),
                Name = "SchoolAdmin",
                NormalizedName = "SCHOOLADMIN",
                ConcurrencyStamp = "role-school-admin-v1"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"),
                Name = "CafeteriaOperator",
                NormalizedName = "CAFETERIAOPERATOR",
                ConcurrencyStamp = "role-cafeteria-operator-v1"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"),
                Name = "Guardian",
                NormalizedName = "GUARDIAN",
                ConcurrencyStamp = "role-guardian-v1"
            });

        modelBuilder.Entity<ZelloaUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasOne(user => user.Tenant)
                .WithMany()
                .HasForeignKey(user => user.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
