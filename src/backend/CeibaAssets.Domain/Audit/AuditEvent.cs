using CeibaAssets.Domain.Common;

namespace CeibaAssets.Domain.Audit;

/// <summary>
/// Evento de auditoría (esquema <c>audit</c>). Es de solo inserción: no se modifica ni se elimina.
/// </summary>
public sealed class AuditEvent
{
    public const int ActorNameMaxLength = 150;
    public const int ActionMaxLength = 64;
    public const int EntityTypeMaxLength = 64;
    public const int EntityIdMaxLength = 64;
    public const int SummaryMaxLength = 500;

    private AuditEvent()
    {
    }

    public Guid Id { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public Guid? ActorUserId { get; private set; }

    public string? ActorName { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public string Summary { get; private set; } = string.Empty;

    public string? DetailsJson { get; private set; }

    public static AuditEvent Create(
        DateTime occurredAtUtc,
        Guid? actorUserId,
        string? actorName,
        string action,
        string entityType,
        string entityId,
        string summary,
        string? detailsJson = null)
    {
        var summaryText = Guard.RequiredText(summary, "El resumen", int.MaxValue);

        return new AuditEvent
        {
            Id = Guid.CreateVersion7(),
            OccurredAtUtc = occurredAtUtc,
            ActorUserId = actorUserId,
            ActorName = Guard.OptionalText(actorName, "El actor", ActorNameMaxLength),
            Action = Guard.RequiredText(action, "La acción", ActionMaxLength),
            EntityType = Guard.RequiredText(entityType, "El tipo de entidad", EntityTypeMaxLength),
            EntityId = Guard.RequiredText(entityId, "El identificador de entidad", EntityIdMaxLength),
            Summary = summaryText.Length > SummaryMaxLength ? summaryText[..SummaryMaxLength] : summaryText,
            DetailsJson = detailsJson,
        };
    }
}
