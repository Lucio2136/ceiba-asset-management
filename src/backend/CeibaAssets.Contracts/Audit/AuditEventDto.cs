namespace CeibaAssets.Contracts.Audit;

/// <summary>Evento de auditoría.</summary>
public sealed record AuditEventDto
{
    public required Guid Id { get; init; }

    public required DateTime OccurredAtUtc { get; init; }

    public Guid? ActorUserId { get; init; }

    public string? ActorName { get; init; }

    public required string Action { get; init; }

    public required string EntityType { get; init; }

    public required string EntityId { get; init; }

    public required string Summary { get; init; }

    public string? DetailsJson { get; init; }
}
