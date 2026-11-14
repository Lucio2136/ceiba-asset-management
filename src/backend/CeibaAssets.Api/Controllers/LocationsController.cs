using CeibaAssets.Api.Mapping;
using CeibaAssets.Api.Security;
using CeibaAssets.Application.Locations;
using CeibaAssets.Contracts.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Ubicaciones físicas (salas, áreas, laboratorios) dentro de los pisos.
/// </summary>
[Route("v1/locations")]
public sealed class LocationsController(ILocationsService locations) : ApiControllerBase
{
    /// <summary>
    /// Lista ubicaciones con filtros opcionales.
    /// </summary>
    /// <param name="buildingId">Filtra por edificio.</param>
    /// <param name="floorId">Filtra por piso.</param>
    /// <param name="search">Texto en nombre o código.</param>
    /// <param name="includeInactive">Incluye ubicaciones inactivas.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LocationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<LocationDto>>> List(
        [FromQuery] Guid? buildingId,
        [FromQuery] Guid? floorId,
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var items = await locations.ListAsync(
            new LocationQuery(buildingId, floorId, search, includeInactive),
            cancellationToken);
        return Ok(items.Select(item => item.ToDto()).ToList());
    }

    /// <summary>
    /// Obtiene una ubicación.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<LocationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await locations.GetAsync(id, cancellationToken), item => item.ToDto());

    /// <summary>
    /// Crea una ubicación (Administrator, AssetManager).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.Write)]
    [ProducesResponseType<LocationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocationDto>> Create(
        CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await locations.CreateAsync(request.ToCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value.ToDto())
            : ToProblem(result.Error!);
    }

    /// <summary>
    /// Edita una ubicación con control de concurrencia (Administrator, AssetManager).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.Write)]
    [ProducesResponseType<LocationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LocationDto>> Update(
        Guid id,
        UpdateLocationRequest request,
        CancellationToken cancellationToken) =>
        ToActionResult(await locations.UpdateAsync(request.ToCommand(id), cancellationToken), item => item.ToDto());
}
