using CeibaAssets.Application.Abstractions;
using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Audit;
using CeibaAssets.Domain.Locations;
using CeibaAssets.Domain.People;
using CeibaAssets.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core. Un esquema SQL por módulo: location, asset, people, assignment, audit y security.
/// </summary>
public sealed class CeibaDbContext(DbContextOptions<CeibaDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public const string LocationSchema = "location";
    public const string AssetSchema = "asset";
    public const string PeopleSchema = "people";
    public const string AssignmentSchema = "assignment";
    public const string AuditSchema = "audit";
    public const string SecuritySchema = "security";

    public DbSet<Building> Buildings => Set<Building>();

    public DbSet<Floor> Floors => Set<Floor>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<AssetIdentifier> AssetIdentifiers => Set<AssetIdentifier>();

    public DbSet<ResponsiblePerson> ResponsiblePeople => Set<ResponsiblePerson>();

    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();

    public DbSet<AssetMovement> AssetMovements => Set<AssetMovement>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Las fechas se guardan y se leen siempre como UTC.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users", SecuritySchema);
            entity.Property(user => user.DisplayName).HasMaxLength(ApplicationUser.DisplayNameMaxLength);
        });
        builder.Entity<ApplicationRole>().ToTable("Roles", SecuritySchema);
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", SecuritySchema);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", SecuritySchema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", SecuritySchema);
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", SecuritySchema);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", SecuritySchema);

        builder.ApplyConfigurationsFromAssembly(typeof(CeibaDbContext).Assembly);
    }
}
