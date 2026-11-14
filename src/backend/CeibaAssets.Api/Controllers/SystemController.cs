using System.Reflection;
using CeibaAssets.Contracts.Platform;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Información pública de la plataforma.
/// </summary>
[ApiController]
[Route("v1/system")]
[Produces("application/json")]
public sealed class SystemController : ControllerBase
{
    private static readonly SystemInfoResponse Info = new()
    {
        ApplicationName = "CEIBA Asset Management",
        Version = ResolveVersion(),
    };

    /// <summary>
    /// Obtiene el nombre y la versión de la API.
    /// </summary>
    [HttpGet("info")]
    [ProducesResponseType<SystemInfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemInfoResponse> GetInfo() => Info;

    private static string ResolveVersion()
    {
        var informationalVersion = typeof(SystemController).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

        // Se omite el metadato "+<commit>" que agrega el SDK.
        var metadataSeparator = informationalVersion.IndexOf('+', StringComparison.Ordinal);
        return metadataSeparator < 0 ? informationalVersion : informationalVersion[..metadataSeparator];
    }
}
