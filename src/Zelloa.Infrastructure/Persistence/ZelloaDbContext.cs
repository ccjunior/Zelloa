using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Zelloa.Domain.Catalog;
using Zelloa.Application.Identity;
using Zelloa.Domain.Guardians;
using Zelloa.Domain.Invitations;
using Zelloa.Domain.Schools;
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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<School> Schools => Set<School>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<GuardianStudent> GuardianStudents => Set<GuardianStudent>();
    public DbSet<AccountInvitation> AccountInvitations => Set<AccountInvitation>();

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

        modelBuilder.Entity<School>(entity =>
        {
            entity.ToTable("Schools");
            entity.HasKey(school => school.Id);
            entity.Property(school => school.Name).HasMaxLength(200).IsRequired();
            entity.Property(school => school.TradeName).HasMaxLength(200).IsRequired();
            entity.Property(school => school.Identifier).HasMaxLength(64);
            entity.Property(school => school.ContactEmail).HasMaxLength(256);
            entity.Property(school => school.ContactPhone).HasMaxLength(32);
            entity.Property(school => school.TimeZone).HasMaxLength(100).IsRequired();
            entity.HasIndex(school => school.TenantId);
            entity.HasQueryFilter(school => school.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(school => school.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(category => category.Id);
            entity.HasAlternateKey(category => new { category.TenantId, category.Id });
            entity.Property(category => category.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(category => new { category.TenantId, category.Name });
            entity.HasQueryFilter(category => category.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(category => category.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products", table => table.HasCheckConstraint("CK_Products_Price_Positive", "\"Price\" > 0"));
            entity.HasKey(product => product.Id);
            entity.HasAlternateKey(product => new { product.TenantId, product.Id });
            entity.Property(product => product.Name).HasMaxLength(200).IsRequired();
            entity.Property(product => product.Description).HasMaxLength(2000);
            entity.Property(product => product.Price).HasPrecision(18, 2);
            entity.Property(product => product.ImageUrl).HasMaxLength(2048);
            entity.HasIndex(product => new { product.TenantId, product.CategoryId, product.IsActive, product.IsAvailable });
            entity.HasQueryFilter(product => product.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(product => product.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Category>().WithMany()
                .HasForeignKey(product => new { product.TenantId, product.CategoryId })
                .HasPrincipalKey(category => new { category.TenantId, category.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.ToTable("Classrooms");
            entity.HasKey(classroom => classroom.Id);
            entity.HasAlternateKey(classroom => new { classroom.TenantId, classroom.Id });
            entity.Property(classroom => classroom.Name).HasMaxLength(200).IsRequired();
            entity.Property(classroom => classroom.Level).HasMaxLength(100).IsRequired();
            entity.Property(classroom => classroom.Shift).HasMaxLength(50).IsRequired();
            entity.HasIndex(classroom => new { classroom.TenantId, classroom.AcademicYear });
            entity.HasQueryFilter(classroom => classroom.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(classroom => classroom.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(student => student.Id);
            entity.HasAlternateKey(student => new { student.TenantId, student.Id });
            entity.Property(student => student.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(student => new { student.TenantId, student.ClassroomId });
            entity.HasQueryFilter(student => student.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(student => student.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Classroom>().WithMany()
                .HasForeignKey(student => new { student.TenantId, student.ClassroomId })
                .HasPrincipalKey(classroom => new { classroom.TenantId, classroom.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Guardian>(entity =>
        {
            entity.ToTable("Guardians");
            entity.HasKey(guardian => guardian.Id);
            entity.HasAlternateKey(guardian => new { guardian.TenantId, guardian.Id });
            entity.Property(guardian => guardian.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(guardian => guardian.TenantId);
            entity.HasQueryFilter(guardian => guardian.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(guardian => guardian.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ZelloaUser>().WithOne()
                .HasForeignKey<Guardian>(guardian => guardian.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GuardianStudent>(entity =>
        {
            entity.ToTable("GuardianStudents");
            entity.HasKey(link => new { link.GuardianId, link.StudentId });
            entity.HasIndex(link => new { link.TenantId, link.StudentId });
            entity.HasQueryFilter(link => link.TenantId == CurrentTenantId);
            entity.HasOne<Tenant>().WithMany().HasForeignKey(link => link.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Guardian>().WithMany()
                .HasForeignKey(link => new { link.TenantId, link.GuardianId })
                .HasPrincipalKey(guardian => new { guardian.TenantId, guardian.Id })
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Student>().WithMany()
                .HasForeignKey(link => new { link.TenantId, link.StudentId })
                .HasPrincipalKey(student => new { student.TenantId, student.Id })
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountInvitation>(entity =>
        {
            entity.ToTable("AccountInvitations");
            entity.HasKey(invitation => invitation.Id);
            entity.Property(invitation => invitation.Role).HasMaxLength(64).IsRequired();
            entity.Property(invitation => invitation.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(invitation => invitation.TokenHash).IsUnique();
            entity.HasIndex(invitation => new { invitation.TenantId, invitation.UserId });
            entity.HasOne<Tenant>().WithMany().HasForeignKey(invitation => invitation.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ZelloaUser>().WithMany().HasForeignKey(invitation => invitation.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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
