using System.Security.Claims;
using CeibaAssets.Application.Abstractions;

namespace CeibaAssets.Api.Security;

/// <summary>
/// Resuelve el usuario de la sesión actual a partir de los claims de Identity.
/// </summary>
internal sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid? UserId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? DisplayName => accessor.HttpContext?.User.Identity?.Name;
}
