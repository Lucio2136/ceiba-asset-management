using CeibaAssets.Application.Common;

namespace CeibaAssets.Application.Locations;

public interface IBuildingsService
{
    Task<IReadOnlyList<BuildingModel>> ListBuildingsAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<FloorModel>>> ListFloorsAsync(Guid buildingId, CancellationToken cancellationToken);
}
