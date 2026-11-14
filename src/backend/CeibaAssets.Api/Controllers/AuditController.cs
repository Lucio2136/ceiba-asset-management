using CeibaAssets.Api.Mapping;
using CeibaAssets.Api.Security;
using CeibaAssets.Application.Audit;
using CeibaAssets.Contracts.Audit;
using CeibaAssets.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Bitácora de auditoría (solo lectura). Requiere Administrator o Auditor.
/// </summary>
[Route("v1/audit-events")]
[Authorize(Policy = Policies.Audit)]
public sealed class AuditController(IAuditService audit) : ApiControllerBase
{
    /// <summary>
    /// Lista eventos de auditoría, los más recientes primero.
    /// </summary>
    /// <param name="entityType">Tipo de entidad (por ejemplo, Asset).</param>
    /// <param name="entityId">Identificador de la entidad.</param>
    /// <param name="action">Acción (por ejemplo, asset.created).</param>
    /// <param name="fromUtc">Desde (UTC).</param>
    /// <param name="toUtc">Hasta (UTC).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<AuditEventDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<AuditEventDto>>> List(
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await audit.ListAsync(
            new AuditEventQuery(entityType, entityId, action, fromUtc, toUtc, page, pageSize),
            cancellationToken);
        return Ok(result.ToDto(item => item.ToDto()));
    }
}
