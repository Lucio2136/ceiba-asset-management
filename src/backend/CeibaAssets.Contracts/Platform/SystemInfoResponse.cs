namespace CeibaAssets.Contracts.Platform;

/// <summary>
/// Identificación pública de la API.
/// </summary>
public sealed record SystemInfoResponse
{
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    public required string ApplicationName { get; init; }

    /// <summary>
    /// Versión semántica de la API.
    /// </summary>
    public required string Version { get; init; }
}
