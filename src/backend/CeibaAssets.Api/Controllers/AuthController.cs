using System.Security.Claims;
using CeibaAssets.Api.Security;
using CeibaAssets.Application.Audit;
using CeibaAssets.Contracts.Auth;
using CeibaAssets.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CeibaAssets.Api.Controllers;

/// <summary>
/// Autenticación con cookie HttpOnly (ADR-0007). Laboratorio: sin IdP externo.
/// </summary>
[Route("v1/auth")]
public sealed class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IAuditTrail audit) : ApiControllerBase
{
    /// <summary>
    /// Inicia sesión. Emite la cookie de autenticación y renueva el token antiforgery.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting(Policies.LoginRateLimit)]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MeResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim() ?? string.Empty;
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await audit.RecordAndSaveAsync(
                "auth.login.failed",
                "User",
                email,
                "Intento de inicio de sesión con correo desconocido.",
                actorUserId: null,
                actorName: email,
                cancellationToken);
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales no válidas",
                detail: "El correo o la contraseña no son correctos.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            await audit.RecordAndSaveAsync(
                "auth.login.locked",
                "User",
                user.Id.ToString(),
                "Cuenta bloqueada por intentos fallidos.",
                user.Id,
                user.DisplayName,
                cancellationToken);
            return Problem(
                statusCode: StatusCodes.Status423Locked,
                title: "Cuenta bloqueada",
                detail: "La cuenta está bloqueada temporalmente por intentos fallidos.");
        }

        if (!result.Succeeded)
        {
            await audit.RecordAndSaveAsync(
                "auth.login.failed",
                "User",
                user.Id.ToString(),
                "Intento de inicio de sesión fallido.",
                user.Id,
                user.DisplayName,
                cancellationToken);
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales no válidas",
                detail: "El correo o la contraseña no son correctos.");
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        await audit.RecordAndSaveAsync(
            "auth.login.succeeded",
            "User",
            user.Id.ToString(),
            "Inicio de sesión correcto.",
            user.Id,
            user.DisplayName,
            cancellationToken);

        XsrfTokenCookie.Issue(HttpContext);
        return Ok(await ToMeResponseAsync(user));
    }

    /// <summary>
    /// Cierra la sesión y limpia la cookie de autenticación.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var displayName = User.Identity?.Name;
        await signInManager.SignOutAsync();
        if (userId is not null)
        {
            await audit.RecordAndSaveAsync(
                "auth.logout",
                "User",
                userId,
                "Cierre de sesión.",
                Guid.TryParse(userId, out var id) ? id : null,
                displayName,
                cancellationToken);
        }

        XsrfTokenCookie.Issue(HttpContext);
        return NoContent();
    }

    /// <summary>
    /// Devuelve el perfil del usuario autenticado.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType<MeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await ToMeResponseAsync(user));
    }

    private async Task<MeResponse> ToMeResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new MeResponse
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            Roles = roles.ToList(),
        };
    }
}
