namespace CeibaAssets.Application.Audit;

public sealed record AuditEventModel(
    Guid Id,
    DateTime OccurredAtUtc,
    Guid? ActorUserId,
    string? ActorName,
    string Action,
    string EntityType,
    string EntityId,
    string Summary,
    string? DetailsJson);

public sealed record AuditEventQuery(
    string? EntityType = null,
    string? EntityId = null,
    string? Action = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Page = 1,
    int PageSize = 25);
