using CeibaAssets.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CeibaAssets.Infrastructure.Persistence.Configurations;

public sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("Buildings", CeibaDbContext.LocationSchema);
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();
        builder.Property(b => b.Code).HasMaxLength(Building.CodeMaxLength).IsRequired();
        builder.Property(b => b.Name).HasMaxLength(Building.NameMaxLength).IsRequired();
        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.HasIndex(b => b.Code).IsUnique();

        builder.HasMany(b => b.Floors)
            .WithOne(f => f.Building)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(b => b.Floors).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("Floors", CeibaDbContext.LocationSchema);
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();
        builder.Property(f => f.Name).HasMaxLength(Floor.NameMaxLength).IsRequired();
        builder.Property(f => f.RowVersion).IsRowVersion();

        builder.HasIndex(f => new { f.BuildingId, f.LevelNumber }).IsUnique();

        builder.HasMany(f => f.Locations)
            .WithOne(l => l.Floor)
            .HasForeignKey(l => l.FloorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(f => f.Locations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations", CeibaDbContext.LocationSchema);
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();
        builder.Property(l => l.Code).HasMaxLength(Location.CodeMaxLength).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(Location.NameMaxLength).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(Location.DescriptionMaxLength);
        builder.Property(l => l.RowVersion).IsRowVersion();

        builder.HasIndex(l => l.Code).IsUnique();
    }
}
