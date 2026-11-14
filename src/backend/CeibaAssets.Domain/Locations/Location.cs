using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Locations;

/// <summary>
/// Ubicación física dentro de un piso (sala, laboratorio, área abierta, etc.).
/// </summary>
public sealed class Location
{
    public const int CodeMaxLength = 40;
    public const int NameMaxLength = 150;
    public const int DescriptionMaxLength = 500;

    private Location()
    {
    }

    public Guid Id { get; private set; }

    public Guid FloorId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = [];

    public Floor? Floor { get; private set; }

    public static Location Create(Guid floorId, string code, string name, string? description) => new()
    {
        Id = Guid.CreateVersion7(),
        FloorId = Guard.RequiredId(floorId, "El piso"),
        Code = NormalizeCode(code),
        Name = Guard.RequiredText(name, "El nombre de la ubicación", NameMaxLength),
        Description = Guard.OptionalText(description, "La descripción", DescriptionMaxLength),
    };

    public static string NormalizeCode(string code) =>
        Guard.RequiredText(code, "El código de la ubicación", CodeMaxLength).ToUpperInvariant();

    public void Update(string name, string? description, bool isActive)
    {
        Name = Guard.RequiredText(name, "El nombre de la ubicación", NameMaxLength);
        Description = Guard.OptionalText(description, "La descripción", DescriptionMaxLength);
        IsActive = isActive;
    }
}
