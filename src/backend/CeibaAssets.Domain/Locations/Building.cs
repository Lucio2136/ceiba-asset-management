using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Locations;

/// <summary>
/// Edificio del campus (esquema <c>location</c>).
/// </summary>
public sealed class Building
{
    public const int CodeMaxLength = 32;
    public const int NameMaxLength = 150;
    public const int MaxLevels = 200;

    private readonly List<Floor> _floors = [];

    private Building()
    {
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int LevelCount { get; private set; }

    public bool IsActive { get; private set; } = true;

    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<Floor> Floors => _floors;

    public static Building Create(string code, string name, int levelCount)
    {
        if (levelCount is < 1 or > MaxLevels)
        {
            throw new DomainException($"La cantidad de niveles debe estar entre 1 y {MaxLevels}.");
        }

        return new Building
        {
            Id = Guid.CreateVersion7(),
            Code = Guard.RequiredText(code, "El código del edificio", CodeMaxLength).ToUpperInvariant(),
            Name = Guard.RequiredText(name, "El nombre del edificio", NameMaxLength),
            LevelCount = levelCount,
        };
    }

    /// <summary>
    /// Agrega el piso <paramref name="levelNumber"/> (1 a <see cref="LevelCount"/>) al edificio.
    /// </summary>
    public Floor AddFloor(int levelNumber, string name)
    {
        if (levelNumber < 1 || levelNumber > LevelCount)
        {
            throw new DomainException($"El nivel debe estar entre 1 y {LevelCount}.");
        }

        if (_floors.Exists(floor => floor.LevelNumber == levelNumber))
        {
            throw new DomainException($"El nivel {levelNumber} ya existe en el edificio.");
        }

        var floor = Floor.Create(Id, levelNumber, name);
        _floors.Add(floor);
        return floor;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
