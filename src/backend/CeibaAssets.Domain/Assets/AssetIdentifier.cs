using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Assets;

/// <summary>
/// Identificador de un activo con su valor original y su valor normalizado para búsquedas.
/// </summary>
public sealed class AssetIdentifier
{
    public const int ValueMaxLength = 128;

    private AssetIdentifier()
    {
    }

    public Guid Id { get; private set; }

    public Guid AssetId { get; private set; }

    public AssetIdentifierKind Kind { get; private set; }

    public string Value { get; private set; } = string.Empty;

    public string NormalizedValue { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public static string Normalize(AssetIdentifierKind kind, string value)
    {
        var trimmed = Guard.RequiredText(value, "El valor del identificador", ValueMaxLength);

        return kind switch
        {
            AssetIdentifierKind.MacAddress or AssetIdentifierKind.Imei =>
                string.Concat(trimmed.Where(char.IsLetterOrDigit)).ToUpperInvariant(),
            _ => trimmed.ToUpperInvariant(),
        };
    }

    internal static AssetIdentifier Create(Guid assetId, AssetIdentifierKind kind, string value, bool isPrimary)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new DomainException("La clase de identificador no es válida.");
        }

        var normalized = Normalize(kind, value);
        if (normalized.Length == 0)
        {
            throw new DomainException("El valor del identificador no es válido.");
        }

        return new AssetIdentifier
        {
            Id = Guid.CreateVersion7(),
            AssetId = assetId,
            Kind = kind,
            Value = value.Trim(),
            NormalizedValue = normalized,
            IsPrimary = isPrimary,
        };
    }
}
