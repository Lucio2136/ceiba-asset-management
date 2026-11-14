namespace CeibaAssets.Application.Common;

/// <summary>
/// Error de negocio que la API traduce a Problem Details.
/// </summary>
public sealed record ApplicationError(
    ErrorKind Kind,
    string Detail,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null);
