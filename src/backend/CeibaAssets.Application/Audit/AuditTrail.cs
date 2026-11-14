using CeibaAssets.Application.Abstractions;
using CeibaAssets.Domain.Audit;

namespace CeibaAssets.Application.Audit;

internal sealed class AuditTrail(IApplicationDbContext db, ICurrentUser currentUser, TimeProvider timeProvider)
    : IAuditTrail
{
    public void Record(string action, string entityType, string entityId, string summary, string? detailsJson = null) =>
        db.AuditEvents.Add(AuditEvent.Create(
            timeProvider.GetUtcNow().UtcDateTime,
            currentUser.UserId,
            currentUser.DisplayName,
            action,
            entityType,
            entityId,
            summary,
            detailsJson));

    public async Task RecordAndSaveAsync(
        string action,
        string entityType,
        string entityId,
        string summary,
        Guid? actorUserId,
        string? actorName,
        CancellationToken cancellationToken)
    {
        db.AuditEvents.Add(AuditEvent.Create(
            timeProvider.GetUtcNow().UtcDateTime,
            actorUserId,
            actorName,
            action,
            entityType,
            entityId,
            summary));

        await db.SaveChangesAsync(cancellationToken);
    }
}
