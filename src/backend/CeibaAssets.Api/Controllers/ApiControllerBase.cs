using CeibaAssets.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Base de los controladores de la API: traduce <see cref="Result{T}"/> a respuestas HTTP con Problem Details.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<TDto> ToActionResult<TModel, TDto>(Result<TModel> result, Func<TModel, TDto> map) =>
        result.IsSuccess ? Ok(map(result.Value)) : ToProblem(result.Error!);

    protected ActionResult ToProblem(ApplicationError error) => error.Kind switch
    {
        ErrorKind.Validation => ValidationProblem(new ValidationProblemDetails(
            (error.ValidationErrors ?? new Dictionary<string, string[]>()).ToDictionary(e => e.Key, e => e.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Solicitud no válida",
            Detail = error.Detail,
        }),
        ErrorKind.NotFound => Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "No encontrado",
            detail: error.Detail),
        _ => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflicto",
            detail: error.Detail),
    };
}
