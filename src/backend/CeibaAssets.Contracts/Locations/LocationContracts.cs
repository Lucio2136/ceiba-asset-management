namespace CeibaAssets.Contracts.Locations;

/// <summary>Edificio del campus.</summary>
public sealed record BuildingDto
{
    public required Guid Id { get; init; }

    public required string Code { get; init; }

    public required string Name { get; init; }

    public required int LevelCount { get; init; }

    public required bool IsActive { get; init; }
}

/// <summary>Piso de un edificio.</summary>
public sealed record FloorDto
{
    public required Guid Id { get; init; }

    public required Guid BuildingId { get; init; }

    public required int LevelNumber { get; init; }

    public required string Name { get; init; }

    public required bool IsActive { get; init; }
}

/// <summary>Ubicación física dentro de un piso.</summary>
public sealed record LocationDto
{
    public required Guid Id { get; init; }

    public required Guid FloorId { get; init; }

    public required string FloorName { get; init; }

    public required int LevelNumber { get; init; }

    public required Guid BuildingId { get; init; }

    public required string BuildingName { get; init; }

    public required string Code { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>Versión de fila (Base64) para concurrencia optimista.</summary>
    public required string RowVersion { get; init; }
}

/// <summary>Datos para crear una ubicación.</summary>
public sealed record CreateLocationRequest
{
    public required Guid FloorId { get; init; }

    /// <summary>Código único (letras, números, punto, guion).</summary>
    public required string Code { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }
}

/// <summary>Datos para editar una ubicación.</summary>
public sealed record UpdateLocationRequest
{
    public required string Name { get; init; }

    public string? Description { get; init; }

    public required bool IsActive { get; init; }

    /// <summary>Versión de fila leída (Base64).</summary>
    public required string RowVersion { get; init; }
}
