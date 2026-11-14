using CeibaAssets.Contracts.Assignments;

namespace CeibaAssets.Contracts.Assets;

/// <summary>Identificador de un activo.</summary>
public sealed record AssetIdentifierDto
{
    public required Guid Id { get; init; }

    public required AssetIdentifierKind Kind { get; init; }

    public required string Value { get; init; }

    public required bool IsPrimary { get; init; }
}

/// <summary>Identificador a registrar en un activo.</summary>
public sealed record AssetIdentifierRequest
{
    public required AssetIdentifierKind Kind { get; init; }

    public required string Value { get; init; }

    public bool IsPrimary { get; init; }
}

/// <summary>Detalle de un activo.</summary>
public sealed record AssetDto
{
    public required Guid Id { get; init; }

    public required AssetType AssetType { get; init; }

    public required AssetStatus Status { get; init; }

    public required string Name { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public string? Notes { get; init; }

    public Guid? LocationId { get; init; }

    public string? LocationName { get; init; }

    /// <summary>Ruta legible: edificio / piso / ubicación.</summary>
    public string? LocationPath { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public required DateTime UpdatedAtUtc { get; init; }

    /// <summary>Versión de fila (Base64) para concurrencia optimista.</summary>
    public required string RowVersion { get; init; }

    public required IReadOnlyList<AssetIdentifierDto> Identifiers { get; init; }

    /// <summary>Asignación vigente, si existe.</summary>
    public AssignmentDto? CurrentAssignment { get; init; }
}

/// <summary>Elemento de la lista de activos.</summary>
public sealed record AssetListItemDto
{
    public required Guid Id { get; init; }

    public required AssetType AssetType { get; init; }

    public required AssetStatus Status { get; init; }

    public required string Name { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public Guid? LocationId { get; init; }

    public string? LocationName { get; init; }

    public string? LocationPath { get; init; }

    public Guid? AssignedToId { get; init; }

    public string? AssignedToName { get; init; }
}

/// <summary>Datos para registrar un activo.</summary>
public sealed record CreateAssetRequest
{
    public required AssetType AssetType { get; init; }

    public required string Name { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    /// <summary>Número de serie. No es único: puede repetirse entre activos.</summary>
    public string? SerialNumber { get; init; }

    public string? Notes { get; init; }

    /// <summary>Ubicación inicial (opcional).</summary>
    public Guid? LocationId { get; init; }

    public IReadOnlyList<AssetIdentifierRequest>? Identifiers { get; init; }
}

/// <summary>Datos para editar un activo.</summary>
public sealed record UpdateAssetRequest
{
    public required AssetType AssetType { get; init; }

    public required string Name { get; init; }

    public string? Manufacturer { get; init; }

    public string? Model { get; init; }

    public string? SerialNumber { get; init; }

    public string? Notes { get; init; }

    /// <summary>Solo Available o InMaintenance. Nulo conserva el estado actual.</summary>
    public AssetStatus? Status { get; init; }

    /// <summary>Nulo conserva los identificadores adicionales actuales.</summary>
    public IReadOnlyList<AssetIdentifierRequest>? Identifiers { get; init; }

    /// <summary>Versión de fila leída (Base64).</summary>
    public required string RowVersion { get; init; }
}

/// <summary>Datos opcionales para dar de baja un activo.</summary>
public sealed record RetireAssetRequest
{
    public string? Reason { get; init; }
}
