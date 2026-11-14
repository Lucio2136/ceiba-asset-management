namespace CeibaAssets.Application.Assignments;

public sealed record AssignmentModel(
    Guid Id,
    Guid AssetId,
    Guid ResponsiblePersonId,
    string ResponsiblePersonName,
    DateTime AssignedAtUtc,
    DateTime? ReturnedAtUtc,
    Guid? AssignedByUserId,
    string? Notes);

public sealed record MovementModel(
    Guid Id,
    Guid AssetId,
    Guid? FromLocationId,
    string? FromLocationName,
    Guid ToLocationId,
    string ToLocationName,
    DateTime MovedAtUtc,
    Guid? MovedByUserId,
    string? Reason);

public sealed record AssignAssetCommand(Guid AssetId, Guid ResponsiblePersonId, string? Notes);

public sealed record ReturnAssetCommand(Guid AssetId, string? Notes);

public sealed record MoveAssetCommand(Guid AssetId, Guid ToLocationId, string? Reason);
