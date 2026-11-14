namespace CeibaAssets.Domain.Assets;

/// <summary>
/// Clase de identificador de un activo. Ningún identificador es único globalmente.
/// </summary>
public enum AssetIdentifierKind
{
    SerialNumber = 1,
    AssetTag = 2,
    MacAddress = 3,
    Imei = 4,
    HostName = 5,
    PhoneExtension = 6,
    Other = 7,
}
