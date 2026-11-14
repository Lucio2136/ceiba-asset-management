using CeibaAssets.Application.Assignments;
using CeibaAssets.Domain.Assets;

namespace CeibaAssets.Application.Assets;

public sealed record AssetIdentifierModel(Guid Id, AssetIdentifierKind Kind, string Value, bool IsPrimary);

public sealed record AssetModel(
    Guid Id,
    AssetType AssetType,
    AssetStatus Status,
    string Name,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Notes,
    Guid? LocationId,
    string? LocationName,
    string? LocationPath,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    string RowVersion,
    IReadOnlyList<AssetIdentifierModel> Identifiers,
    AssignmentModel? CurrentAssignment);

public sealed record AssetListItemModel(
    Guid Id,
    AssetType AssetType,
    AssetStatus Status,
    string Name,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    Guid? LocationId,
    string? LocationName,
    string? LocationPath,
    Guid? AssignedToId,
    string? AssignedToName);

public sealed record AssetListQuery(
    string? Search = null,
    AssetType? AssetType = null,
    AssetStatus? Status = null,
    Guid? LocationId = null,
    Guid? AssignedTo = null,
    int Page = 1,
    int PageSize = 25);

public sealed record AssetIdentifierCommand(AssetIdentifierKind Kind, string Value, bool IsPrimary);

public sealed record CreateAssetCommand(
    AssetType AssetType,
    string Name,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Notes,
    Guid? LocationId,
    IReadOnlyList<AssetIdentifierCommand>? Identifiers);

/// <summary>
/// <see cref="Identifiers"/> nulo conserva los identificadores adicionales actuales.
/// <see cref="Status"/> solo admite Available o InMaintenance.
/// </summary>
public sealed record UpdateAssetCommand(
    Guid Id,
    AssetType AssetType,
    string Name,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Notes,
    AssetStatus? Status,
    IReadOnlyList<AssetIdentifierCommand>? Identifiers,
    string RowVersion);

public sealed record RetireAssetCommand(Guid Id, string? Reason);
