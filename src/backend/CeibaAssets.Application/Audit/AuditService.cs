using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace CeibaAssets.Application.Audit;

internal sealed class AuditService(IApplicationDbContext db) : IAuditService
{
    public async Task<PagedModel<AuditEventModel>> ListAsync(AuditEventQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var events = db.AuditEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var entityType = query.EntityType.Trim();
            events = events.Where(e => e.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            var entityId = query.EntityId.Trim();
            events = events.Where(e => e.EntityId == entityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var action = query.Action.Trim();
            events = events.Where(e => e.Action == action);
        }

        if (query.FromUtc is { } from)
        {
            events = events.Where(e => e.OccurredAtUtc >= from);
        }

        if (query.ToUtc is { } to)
        {
            events = events.Where(e => e.OccurredAtUtc <= to);
        }

        var total = await events.CountAsync(cancellationToken);
        var items = await events
            .OrderByDescending(e => e.OccurredAtUtc)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new AuditEventModel(
                e.Id,
                e.OccurredAtUtc,
                e.ActorUserId,
                e.ActorName,
                e.Action,
                e.EntityType,
                e.EntityId,
                e.Summary,
                e.DetailsJson))
            .ToListAsync(cancellationToken);

        return new PagedModel<AuditEventModel>(items, total, page, pageSize);
    }
}
