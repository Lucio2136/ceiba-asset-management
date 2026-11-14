using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Assignments;

/// <summary>
/// Cambio de ubicación de un activo (esquema <c>assignment</c>); es un registro histórico.
/// </summary>
public sealed class AssetMovement
{
    public const int ReasonMaxLength = 500;

    private AssetMovement()
    {
    }

    public Guid Id { get; private set; }

    public Guid AssetId { get; private set; }

    public Guid? FromLocationId { get; private set; }

    public Guid ToLocationId { get; private set; }

    public DateTime MovedAtUtc { get; private set; }

    public Guid? MovedByUserId { get; private set; }

    public string? Reason { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public static AssetMovement Create(
        Guid assetId,
        Guid? fromLocationId,
        Guid toLocationId,
        DateTime nowUtc,
        Guid? movedByUserId,
        string? reason) => new()
        {
            Id = Guid.CreateVersion7(),
            AssetId = Guard.RequiredId(assetId, "El activo"),
            FromLocationId = fromLocationId,
            ToLocationId = Guard.RequiredId(toLocationId, "La ubicación de destino"),
            MovedAtUtc = nowUtc,
            MovedByUserId = movedByUserId,
            Reason = Guard.OptionalText(reason, "El motivo", ReasonMaxLength),
        };
}
