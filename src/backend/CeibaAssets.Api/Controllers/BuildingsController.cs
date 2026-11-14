using CeibaAssets.Api.Mapping;
using CeibaAssets.Application.Locations;
using CeibaAssets.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Edificios y pisos del campus.
/// </summary>
[Route("v1/buildings")]
public sealed class BuildingsController(IBuildingsService buildings) : ApiControllerBase
{
    /// <summary>
    /// Lista los edificios.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<BuildingDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BuildingDto>>> List(CancellationToken cancellationToken)
    {
        var items = await buildings.ListBuildingsAsync(cancellationToken);
        return Ok(items.Select(item => item.ToDto()).ToList());
    }

    /// <summary>
    /// Lista los pisos de un edificio.
    /// </summary>
    [HttpGet("{id:guid}/floors")]
    [ProducesResponseType<IReadOnlyList<FloorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<FloorDto>>> ListFloors(Guid id, CancellationToken cancellationToken)
    {
        var result = await buildings.ListFloorsAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value.Select(item => item.ToDto()).ToList())
            : ToProblem(result.Error!);
    }
}
