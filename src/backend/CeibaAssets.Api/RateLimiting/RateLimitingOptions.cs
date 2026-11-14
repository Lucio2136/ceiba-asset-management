using System.ComponentModel.DataAnnotations;

namespace CeibaAssets.Api.RateLimiting;

/// <summary>
/// Límite global por dirección IP de cliente, en ventana fija.
/// </summary>
internal sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, 100_000)]
    public int PermitLimit { get; init; } = 300;

    [Range(1, 3_600)]
    public int WindowSeconds { get; init; } = 60;

    /// <summary>Intentos de inicio de sesión permitidos por IP en cada ventana (ADR-0007).</summary>
    [Range(1, 100_000)]
    public int LoginPermitLimit { get; init; } = 10;
}
