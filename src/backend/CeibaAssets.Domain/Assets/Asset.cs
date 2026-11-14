using CeibaAssets.Domain.Common;
using CeibaAssets.Domain.Locations;

namespace CeibaAssets.Domain.Assets;

/// <summary>
/// Activo tecnológico. El número de serie no es único global: puede repetirse entre activos.
/// </summary>
public sealed class Asset
{
    public const int NameMaxLength = 200;
    public const int ManufacturerMaxLength = 100;
    public const int ModelMaxLength = 100;
    public const int SerialNumberMaxLength = AssetIdentifier.ValueMaxLength;
    public const int NotesMaxLength = 2000;

    private readonly List<AssetIdentifier> _identifiers = [];

    private Asset()
    {
    }

    public Guid Id { get; private set; }

    public AssetType AssetType { get; private set; }

    public AssetStatus Status { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Manufacturer { get; private set; }

    public string? Model { get; private set; }

    public string? SerialNumber { get; private set; }

    public string? Notes { get; private set; }

    public Guid? LocationId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public Location? Location { get; private set; }

    public IReadOnlyCollection<AssetIdentifier> Identifiers => _identifiers;

    public static Asset Create(
        AssetType assetType,
        string name,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        Guid? locationId,
        IEnumerable<AssetIdentifierInput>? identifiers,
        DateTime nowUtc)
    {
        var asset = new Asset
        {
            Id = Guid.CreateVersion7(),
            Status = AssetStatus.Available,
            LocationId = locationId == Guid.Empty ? null : locationId,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc,
        };

        asset.ApplyDetails(assetType, name, manufacturer, model, serialNumber, notes);
        asset.ReplaceIdentifiers(identifiers);
        return asset;
    }

    public void UpdateDetails(
        AssetType assetType,
        string name,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes,
        IEnumerable<AssetIdentifierInput>? identifiers,
        DateTime nowUtc)
    {
        EnsureNotRetired();
        ApplyDetails(assetType, name, manufacturer, model, serialNumber, notes);
        ReplaceIdentifiers(identifiers);
        UpdatedAtUtc = nowUtc;
    }

    public void MarkAssigned(DateTime nowUtc)
    {
        if (Status != AssetStatus.Available)
        {
            throw new DomainException("Solo se puede asignar un activo disponible.");
        }

        Status = AssetStatus.Assigned;
        UpdatedAtUtc = nowUtc;
    }

    public void MarkReturned(DateTime nowUtc)
    {
        if (Status != AssetStatus.Assigned)
        {
            throw new DomainException("El activo no está asignado.");
        }

        Status = AssetStatus.Available;
        UpdatedAtUtc = nowUtc;
    }

    public void SendToMaintenance(DateTime nowUtc)
    {
        if (Status != AssetStatus.Available)
        {
            throw new DomainException("Solo un activo disponible puede enviarse a mantenimiento.");
        }

        Status = AssetStatus.InMaintenance;
        UpdatedAtUtc = nowUtc;
    }

    public void ReleaseFromMaintenance(DateTime nowUtc)
    {
        if (Status != AssetStatus.InMaintenance)
        {
            throw new DomainException("El activo no está en mantenimiento.");
        }

        Status = AssetStatus.Available;
        UpdatedAtUtc = nowUtc;
    }

    public void Retire(DateTime nowUtc)
    {
        if (Status == AssetStatus.Retired)
        {
            throw new DomainException("El activo ya está dado de baja.");
        }

        if (Status == AssetStatus.Assigned)
        {
            throw new DomainException("No se puede dar de baja un activo asignado; devuélvalo primero.");
        }

        Status = AssetStatus.Retired;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>
    /// Cambia la ubicación del activo y devuelve la ubicación anterior.
    /// </summary>
    public Guid? MoveTo(Guid locationId, DateTime nowUtc)
    {
        EnsureNotRetired();
        Guard.RequiredId(locationId, "La ubicación de destino");

        if (LocationId == locationId)
        {
            throw new DomainException("El activo ya se encuentra en esa ubicación.");
        }

        var previous = LocationId;
        LocationId = locationId;
        UpdatedAtUtc = nowUtc;
        return previous;
    }

    private void EnsureNotRetired()
    {
        if (Status == AssetStatus.Retired)
        {
            throw new DomainException("Un activo dado de baja no puede modificarse.");
        }
    }

    private void ApplyDetails(
        AssetType assetType,
        string name,
        string? manufacturer,
        string? model,
        string? serialNumber,
        string? notes)
    {
        if (!Enum.IsDefined(assetType))
        {
            throw new DomainException("El tipo de activo no es válido.");
        }

        AssetType = assetType;
        Name = Guard.RequiredText(name, "El nombre del activo", NameMaxLength);
        Manufacturer = Guard.OptionalText(manufacturer, "El fabricante", ManufacturerMaxLength);
        Model = Guard.OptionalText(model, "El modelo", ModelMaxLength);
        SerialNumber = Guard.OptionalText(serialNumber, "El número de serie", SerialNumberMaxLength);
        Notes = Guard.OptionalText(notes, "Las notas", NotesMaxLength);
    }

    private void ReplaceIdentifiers(IEnumerable<AssetIdentifierInput>? identifiers)
    {
        var inputs = (identifiers ?? []).ToList();

        // El número de serie siempre se refleja como identificador principal de su clase.
        if (SerialNumber is not null)
        {
            inputs.RemoveAll(input => input.Kind == AssetIdentifierKind.SerialNumber
                && AssetIdentifier.Normalize(input.Kind, input.Value) == AssetIdentifier.Normalize(
                    AssetIdentifierKind.SerialNumber,
                    SerialNumber));
            inputs.Insert(0, new AssetIdentifierInput(AssetIdentifierKind.SerialNumber, SerialNumber, true));
        }

        var primaryKinds = inputs.Where(input => input.IsPrimary).GroupBy(input => input.Kind);
        if (primaryKinds.Any(group => group.Count() > 1))
        {
            throw new DomainException("Solo puede haber un identificador principal por clase.");
        }

        var created = inputs.Select(input => AssetIdentifier.Create(Id, input.Kind, input.Value, input.IsPrimary)).ToList();
        _identifiers.Clear();
        _identifiers.AddRange(created);
    }
}
