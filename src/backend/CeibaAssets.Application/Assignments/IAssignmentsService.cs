using CeibaAssets.Application.Common;

namespace CeibaAssets.Application.Assignments;

public interface IAssignmentsService
{
    Task<Result<AssignmentModel>> AssignAsync(AssignAssetCommand command, CancellationToken cancellationToken);

    Task<Result<AssignmentModel>> ReturnAsync(ReturnAssetCommand command, CancellationToken cancellationToken);

    Task<Result<MovementModel>> MoveAsync(MoveAssetCommand command, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<AssignmentModel>>> ListAssignmentsAsync(Guid assetId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<MovementModel>>> ListMovementsAsync(Guid assetId, CancellationToken cancellationToken);
}
