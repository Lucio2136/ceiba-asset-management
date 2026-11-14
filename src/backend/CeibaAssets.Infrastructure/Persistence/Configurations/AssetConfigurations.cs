using CeibaAssets.Domain.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CeibaAssets.Infrastructure.Persistence.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets", CeibaDbContext.AssetSchema);
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.AssetType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(Asset.NameMaxLength).IsRequired();
        builder.Property(a => a.Manufacturer).HasMaxLength(Asset.ManufacturerMaxLength);
        builder.Property(a => a.Model).HasMaxLength(Asset.ModelMaxLength);
        builder.Property(a => a.SerialNumber).HasMaxLength(Asset.SerialNumberMaxLength);
        builder.Property(a => a.Notes).HasMaxLength(Asset.NotesMaxLength);
        builder.Property(a => a.RowVersion).IsRowVersion();

        // El número de serie NO es único global: el índice solo acelera la búsqueda.
        builder.HasIndex(a => a.SerialNumber);
        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => new { a.Status, a.AssetType });
        builder.HasIndex(a => a.LocationId);

        builder.HasOne(a => a.Location)
            .WithMany()
            .HasForeignKey(a => a.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Identifiers)
            .WithOne()
            .HasForeignKey(i => i.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Identifiers).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class AssetIdentifierConfiguration : IEntityTypeConfiguration<AssetIdentifier>
{
    public void Configure(EntityTypeBuilder<AssetIdentifier> builder)
    {
        builder.ToTable("AssetIdentifiers", CeibaDbContext.AssetSchema);
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(i => i.Value).HasMaxLength(AssetIdentifier.ValueMaxLength).IsRequired();
        builder.Property(i => i.NormalizedValue).HasMaxLength(AssetIdentifier.ValueMaxLength).IsRequired();

        // Sin unicidad: varios activos pueden compartir un identificador (por ejemplo, un serial repetido).
        builder.HasIndex(i => new { i.Kind, i.NormalizedValue });
        builder.HasIndex(i => i.AssetId);
    }
}
