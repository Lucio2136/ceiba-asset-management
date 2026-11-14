using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Locations;

/// <summary>
/// Piso o nivel de un edificio.
/// </summary>
public sealed class Floor
{
    public const int NameMaxLength = 100;

    private readonly List<Location> _locations = [];

    private Floor()
    {
    }

    public Guid Id { get; private set; }

    public Guid BuildingId { get; private set; }

    public int LevelNumber { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = [];

    public Building? Building { get; private set; }

    public IReadOnlyCollection<Location> Locations => _locations;

    internal static Floor Create(Guid buildingId, int levelNumber, string name)
    {
        if (levelNumber < 1)
        {
            throw new DomainException("El nivel debe ser mayor o igual a 1.");
        }

        return new Floor
        {
            Id = Guid.CreateVersion7(),
            BuildingId = Guard.RequiredId(buildingId, "El edificio"),
            LevelNumber = levelNumber,
            Name = Guard.RequiredText(name, "El nombre del piso", NameMaxLength),
        };
    }

    public Location AddLocation(string code, string name, string? description)
    {
        var location = Location.Create(Id, code, name, description);
        _locations.Add(location);
        return location;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
