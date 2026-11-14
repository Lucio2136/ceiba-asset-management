using CeibaAssets.Api.Mapping;
using CeibaAssets.Api.Security;
using CeibaAssets.Application.Assets;
using CeibaAssets.Application.Assignments;
using CeibaAssets.Contracts.Assets;
using CeibaAssets.Contracts.Assignments;
using CeibaAssets.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DomainAssets = CeibaAssets.Domain.Assets;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Inventario de activos tecnológicos.
/// </summary>
[Route("v1/assets")]
public sealed class AssetsController(IAssetsService assets, IAssignmentsService assignments) : ApiControllerBase
{
    /// <summary>
    /// Lista activos con paginación y filtros.
    /// </summary>
    /// <param name="search">Texto en nombre, fabricante, modelo, serie o identificadores.</param>
    /// <param name="type">Tipo de activo.</param>
    /// <param name="status">Estado.</param>
    /// <param name="locationId">Ubicación actual.</param>
    /// <param name="assignedTo">Responsable con asignación vigente.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<AssetListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<AssetListItemDto>>> List(
        [FromQuery] string? search,
        [FromQuery] AssetType? type,
        [FromQuery] AssetStatus? status,
        [FromQuery] Guid? locationId,
        [FromQuery] Guid? assignedTo,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var query = new AssetListQuery(
            search,
            type is { } t ? ContractMapper.ConvertEnum<AssetType, DomainAssets.AssetType>(t) : null,
            status is { } s ? ContractMapper.ConvertEnum<AssetStatus, DomainAssets.AssetStatus>(s) : null,
            locationId,
            assignedTo,
            page,
            pageSize);

        var result = await assets.ListAsync(query, cancellationToken);
        return Ok(result.ToDto(item => item.ToDto()));
    }

    /// <summary>
    /// Obtiene el detalle de un activo.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await assets.GetAsync(id, cancellationToken), item => item.ToDto());

    /// <summary>
    /// Registra un activo (Administrator, AssetManager). El número de serie puede repetirse.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.Write)]
    [ProducesResponseType<AssetDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Create(CreateAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await assets.CreateAsync(request.ToCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value.ToDto())
            : ToProblem(result.Error!);
    }

    /// <summary>
    /// Edita un activo con control de concurrencia (Administrator, AssetManager).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Write)]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Update(
        Guid id,
        UpdateAssetRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await assets.UpdateAsync(request.ToCommand(id), cancellationToken), item => item.ToDto());

    /// <summary>
    /// Da de baja un activo (Administrator, AssetManager). No procede si está asignado.
    /// </summary>
    [HttpPost("{id:guid}/retire")]
    [Authorize(Policy = Policies.Write)]
    [ProducesResponseType<AssetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssetDto>> Retire(
        Guid id,
        CancellationToken cancellationToken,
        [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RetireAssetRequest? request = null) =>
        ToActionResult(
            await assets.RetireAsync(new RetireAssetCommand(id, request?.Reason), cancellationToken),
            item => item.ToDto());

    /// <summary>
    /// Historial de asignaciones de un activo (la más reciente primero).
    /// </summary>
    [HttpGet("{id:guid}/assignments")]
    [ProducesResponseType<IReadOnlyList<AssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> ListAssignments(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await assignments.ListAssignmentsAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value.Select(item => item.ToDto()).ToList())
            : ToProblem(result.Error!);
    }

    /// <summary>
    /// Historial de movimientos de ubicación de un activo (el más reciente primero).
    /// </summary>
    [HttpGet("{id:guid}/movements")]
    [ProducesResponseType<IReadOnlyList<MovementDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MovementDto>>> ListMovements(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await assignments.ListMovementsAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value.Select(item => item.ToDto()).ToList())
            : ToProblem(result.Error!);
    }
}
