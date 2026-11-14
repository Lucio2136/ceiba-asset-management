namespace CeibaAssets.Domain.Assets;

/// <summary>
/// Datos de entrada para registrar un identificador en un activo.
/// </summary>
public readonly record struct AssetIdentifierInput(AssetIdentifierKind Kind, string Value, bool IsPrimary);
