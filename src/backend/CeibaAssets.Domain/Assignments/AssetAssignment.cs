using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Assignments;

/// <summary>
/// Asignación de un activo a un responsable (esquema <c>assignment</c>). Una asignación abierta
/// tiene <see cref="ReturnedAtUtc"/> nulo; un activo solo puede tener una asignación abierta.
/// </summary>
public sealed class AssetAssignment
{
    public const int NotesMaxLength = 1000;

    private AssetAssignment()
    {
    }

    public Guid Id { get; private set; }

    public Guid AssetId { get; private set; }

    public Guid ResponsiblePersonId { get; private set; }

    public DateTime AssignedAtUtc { get; private set; }

    public DateTime? ReturnedAtUtc { get; private set; }

    public Guid? AssignedByUserId { get; private set; }

    public string? Notes { get; private set; }

    public byte[] RowVersion { get; private set; } = [];

    public bool IsOpen => ReturnedAtUtc is null;

    public static AssetAssignment Create(
        Guid assetId,
        Guid responsiblePersonId,
        DateTime nowUtc,
        Guid? assignedByUserId,
        string? notes) => new()
        {
            Id = Guid.CreateVersion7(),
            AssetId = Guard.RequiredId(assetId, "El activo"),
            ResponsiblePersonId = Guard.RequiredId(responsiblePersonId, "El responsable"),
            AssignedAtUtc = nowUtc,
            AssignedByUserId = assignedByUserId,
            Notes = Guard.OptionalText(notes, "Las notas", NotesMaxLength),
        };

    public void Return(DateTime nowUtc, string? returnNotes)
    {
        if (!IsOpen)
        {
            throw new DomainException("La asignación ya fue devuelta.");
        }

        ReturnedAtUtc = nowUtc;

        var extra = Guard.OptionalText(returnNotes, "Las notas de devolución", NotesMaxLength);
        if (extra is not null)
        {
            var combined = Notes is null ? extra : $"{Notes} | Devolución: {extra}";
            Notes = combined.Length > NotesMaxLength ? combined[..NotesMaxLength] : combined;
        }
    }
}
