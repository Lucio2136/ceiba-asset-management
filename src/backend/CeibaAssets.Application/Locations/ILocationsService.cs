using CeibaAssets.Application.Common;

namespace CeibaAssets.Application.Locations;

public interface ILocationsService
{
    Task<IReadOnlyList<LocationModel>> ListAsync(LocationQuery query, CancellationToken cancellationToken);

    Task<Result<LocationModel>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<LocationModel>> CreateAsync(CreateLocationCommand command, CancellationToken cancellationToken);

    Task<Result<LocationModel>> UpdateAsync(UpdateLocationCommand command, CancellationToken cancellationToken);
}
