namespace CeibaAssets.Domain.Assets;

/// <summary>
/// Estado del ciclo de vida de un activo.
/// </summary>
public enum AssetStatus
{
    Available = 1,
    Assigned = 2,
    InMaintenance = 3,
    Retired = 4,
}
