using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.Locations;

internal sealed class BuildingsService(IApplicationDbContext db) : IBuildingsService
{
    public async Task<IReadOnlyList<BuildingModel>> ListBuildingsAsync(CancellationToken cancellationToken) =>
        await db.Buildings
            .AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new BuildingModel(b.Id, b.Code, b.Name, b.LevelCount, b.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<Result<IReadOnlyList<FloorModel>>> ListFloorsAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        if (!await db.Buildings.AnyAsync(b => b.Id == buildingId, cancellationToken))
        {
            return Result.NotFound<IReadOnlyList<FloorModel>>("El edificio no existe.");
        }

        var floors = await db.Floors
            .AsNoTracking()
            .Where(f => f.BuildingId == buildingId)
            .OrderBy(f => f.LevelNumber)
            .Select(f => new FloorModel(f.Id, f.BuildingId, f.LevelNumber, f.Name, f.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<FloorModel>>(floors);
    }
}
