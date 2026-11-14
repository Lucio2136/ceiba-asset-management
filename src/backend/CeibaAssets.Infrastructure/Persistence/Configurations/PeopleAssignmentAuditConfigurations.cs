using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Audit;
using CeibaAssets.Domain.Locations;
using CeibaAssets.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CeibaAssets.Infrastructure.Persistence.Configurations;

public sealed class ResponsiblePersonConfiguration : IEntityTypeConfiguration<ResponsiblePerson>
{
    public void Configure(EntityTypeBuilder<ResponsiblePerson> builder)
    {
        builder.ToTable("ResponsiblePeople", CeibaDbContext.PeopleSchema);
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.EmployeeNumber).HasMaxLength(ResponsiblePerson.EmployeeNumberMaxLength).IsRequired();
        builder.Property(p => p.DisplayName).HasMaxLength(ResponsiblePerson.DisplayNameMaxLength).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(ResponsiblePerson.EmailMaxLength).IsRequired();
        builder.Property(p => p.Department).HasMaxLength(ResponsiblePerson.DepartmentMaxLength);
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasIndex(p => p.EmployeeNumber).IsUnique();
        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.DisplayName);
    }
}

public sealed class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{
    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.ToTable("AssetAssignments", CeibaDbContext.AssignmentSchema);
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Notes).HasMaxLength(AssetAssignment.NotesMaxLength);
        builder.Property(a => a.RowVersion).IsRowVersion();
        builder.Ignore(a => a.IsOpen);

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(a => a.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ResponsiblePerson>()
            .WithMany()
            .HasForeignKey(a => a.ResponsiblePersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un activo solo puede tener una asignación vigente (ReturnedAtUtc nulo).
        builder.HasIndex(a => a.AssetId)
            .IsUnique()
            .HasFilter("[ReturnedAtUtc] IS NULL")
            .HasDatabaseName("UX_AssetAssignments_OpenPerAsset");
        builder.HasIndex(a => a.ResponsiblePersonId);
    }
}

public sealed class AssetMovementConfiguration : IEntityTypeConfiguration<AssetMovement>
{
    public void Configure(EntityTypeBuilder<AssetMovement> builder)
    {
        builder.ToTable("AssetMovements", CeibaDbContext.AssignmentSchema);
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Reason).HasMaxLength(AssetMovement.ReasonMaxLength);
        builder.Property(m => m.RowVersion).IsRowVersion();

        builder.HasOne<Asset>()
            .WithMany()
            .HasForeignKey(m => m.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(m => m.FromLocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Location>()
            .WithMany()
            .HasForeignKey(m => m.ToLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.AssetId, m.MovedAtUtc });
    }
}

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents", CeibaDbContext.AuditSchema);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.ActorName).HasMaxLength(AuditEvent.ActorNameMaxLength);
        builder.Property(e => e.Action).HasMaxLength(AuditEvent.ActionMaxLength).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(AuditEvent.EntityTypeMaxLength).IsRequired();
        builder.Property(e => e.EntityId).HasMaxLength(AuditEvent.EntityIdMaxLength).IsRequired();
        builder.Property(e => e.Summary).HasMaxLength(AuditEvent.SummaryMaxLength).IsRequired();

        builder.HasIndex(e => e.OccurredAtUtc);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
    }
}
