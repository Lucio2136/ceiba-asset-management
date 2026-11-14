using CeibaAssets.Application.Common;

namespace CeibaAssets.Application.Audit;

public interface IAuditService
{
    Task<PagedModel<AuditEventModel>> ListAsync(AuditEventQuery query, CancellationToken cancellationToken);
}
