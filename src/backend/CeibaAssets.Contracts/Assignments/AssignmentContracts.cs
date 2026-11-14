namespace CeibaAssets.Contracts.Assignments;

/// <summary>Asignación de un activo a un responsable.</summary>
public sealed record AssignmentDto
{
    public required Guid Id { get; init; }

    public required Guid AssetId { get; init; }

    public required Guid ResponsiblePersonId { get; init; }

    public required string ResponsiblePersonName { get; init; }

    public required DateTime AssignedAtUtc { get; init; }

    public DateTime? ReturnedAtUtc { get; init; }

    public Guid? AssignedByUserId { get; init; }

    public string? Notes { get; init; }
}

/// <summary>Cambio de ubicación de un activo.</summary>
public sealed record MovementDto
{
    public required Guid Id { get; init; }

    public required Guid AssetId { get; init; }

    public Guid? FromLocationId { get; init; }

    public string? FromLocationName { get; init; }

    public required Guid ToLocationId { get; init; }

    public required string ToLocationName { get; init; }

    public required DateTime MovedAtUtc { get; init; }

    public Guid? MovedByUserId { get; init; }

    public string? Reason { get; init; }
}

/// <summary>Datos para asignar un activo.</summary>
public sealed record AssignAssetRequest
{
    public required Guid AssetId { get; init; }

    public required Guid ResponsiblePersonId { get; init; }

    public string? Notes { get; init; }
}

/// <summary>Datos para devolver un activo.</summary>
public sealed record ReturnAssetRequest
{
    public required Guid AssetId { get; init; }

    public string? Notes { get; init; }
}

/// <summary>Datos para mover un activo de ubicación.</summary>
public sealed record MoveAssetRequest
{
    public required Guid AssetId { get; init; }

    public required Guid ToLocationId { get; init; }

    public string? Reason { get; init; }
}
