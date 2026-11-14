namespace CeibaAssets.Contracts.Assets;

/// <summary>Tipo de activo.</summary>
public enum AssetType
{
    Computer = 1,
    Laptop = 2,
    Monitor = 3,
    Webcam = 4,
    Peripheral = 5,
    CiscoPhone = 6,
    MobileDevice = 7,
}

/// <summary>Estado del activo.</summary>
public enum AssetStatus
{
    Available = 1,
    Assigned = 2,
    InMaintenance = 3,
    Retired = 4,
}

/// <summary>Clase de identificador de un activo.</summary>
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
