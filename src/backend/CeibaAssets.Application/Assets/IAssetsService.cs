using CeibaAssets.Application.Common;

namespace CeibaAssets.Application.Assets;

public interface IAssetsService
{
    Task<PagedModel<AssetListItemModel>> ListAsync(AssetListQuery query, CancellationToken cancellationToken);

    Task<Result<AssetModel>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<AssetModel>> CreateAsync(CreateAssetCommand command, CancellationToken cancellationToken);

    Task<Result<AssetModel>> UpdateAsync(UpdateAssetCommand command, CancellationToken cancellationToken);

    Task<Result<AssetModel>> RetireAsync(RetireAssetCommand command, CancellationToken cancellationToken);
}
