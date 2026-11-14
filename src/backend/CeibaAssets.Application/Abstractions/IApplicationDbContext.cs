using CeibaAssets.Domain.Assets;
using CeibaAssets.Domain.Assignments;
using CeibaAssets.Domain.Audit;
using CeibaAssets.Domain.Locations;
using CeibaAssets.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CeibaAssets.Application.Abstractions;

/// <summary>
/// Puerto de persistencia que Infrastructure implementa con EF Core.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Building> Buildings { get; }

    DbSet<Floor> Floors { get; }

    DbSet<Location> Locations { get; }

    DbSet<Asset> Assets { get; }

    DbSet<AssetIdentifier> AssetIdentifiers { get; }

    DbSet<ResponsiblePerson> ResponsiblePeople { get; }

    DbSet<AssetAssignment> AssetAssignments { get; }

    DbSet<AssetMovement> AssetMovements { get; }

    DbSet<AuditEvent> AuditEvents { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
