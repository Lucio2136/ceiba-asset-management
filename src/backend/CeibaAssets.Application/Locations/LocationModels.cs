namespace CeibaAssets.Application.Locations;

public sealed record BuildingModel(Guid Id, string Code, string Name, int LevelCount, bool IsActive);

public sealed record FloorModel(Guid Id, Guid BuildingId, int LevelNumber, string Name, bool IsActive);

public sealed record LocationModel(
    Guid Id,
    Guid FloorId,
    string FloorName,
    int LevelNumber,
    Guid BuildingId,
    string BuildingName,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    string RowVersion);

public sealed record LocationQuery(
    Guid? BuildingId = null,
    Guid? FloorId = null,
    string? Search = null,
    bool IncludeInactive = false);

public sealed record CreateLocationCommand(Guid FloorId, string Code, string Name, string? Description);

public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    string RowVersion);
