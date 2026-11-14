using CeibaAssets.Api.Mapping;
using CeibaAssets.Api.Security;
using CeibaAssets.Application.Assignments;
using CeibaAssets.Contracts.Assignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Asignación, devolución y cambio de ubicación de activos. Requiere Administrator o AssetManager.
/// </summary>
[Route("v1/assignments")]
[Authorize(Policy = Policies.Write)]
public sealed class AssignmentsController(IAssignmentsService assignments) : ApiControllerBase
{
    /// <summary>
    /// Asigna un activo disponible a un responsable.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignmentDto>> Assign(AssignAssetRequest request, CancellationToken cancellationToken)
    {
        var result = await assignments.AssignAsync(request.ToCommand(), cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value.ToDto())
            : ToProblem(result.Error!);
    }

    /// <summary>
    /// Registra la devolución de un activo asignado.
    /// </summary>
    [HttpPost("return")]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignmentDto>> Return(ReturnAssetRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.ReturnAsync(request.ToCommand(), cancellationToken), item => item.ToDto());

    /// <summary>
    /// Mueve un activo a otra ubicación y registra el movimiento.
    /// </summary>
    [HttpPost("move")]
    [ProducesResponseType<MovementDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MovementDto>> Move(MoveAssetRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.MoveAsync(request.ToCommand(), cancellationToken), item => item.ToDto());
}
