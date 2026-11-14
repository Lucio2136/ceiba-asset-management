using CeibaAssets.Api.Mapping;
using CeibaAssets.Application.People;
using CeibaAssets.Contracts.Common;
using CeibaAssets.Contracts.People;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Responsables de activos.
/// </summary>
[Route("v1/responsible-people")]
public sealed class ResponsiblePeopleController(IResponsiblePeopleService people) : ApiControllerBase
{
    /// <summary>
    /// Lista responsables con búsqueda y paginación.
    /// </summary>
    /// <param name="search">Texto en nombre, correo, número de empleado o departamento.</param>
    /// <param name="includeInactive">Incluye responsables inactivos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <param name="page">Página (desde 1).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    [HttpGet]
    [ProducesResponseType<PagedResult<ResponsiblePersonDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ResponsiblePersonDto>>> List(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var result = await people.ListAsync(
            new ResponsiblePersonQuery(search, includeInactive, page, pageSize),
            cancellationToken);
        return Ok(result.ToDto(item => item.ToDto()));
    }

    /// <summary>
    /// Obtiene un responsable.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ResponsiblePersonDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResponsiblePersonDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await people.GetAsync(id, cancellationToken), item => item.ToDto());
}
