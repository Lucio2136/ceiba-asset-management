using Microsoft.AspNetCore.Antiforgery;

namespace CeibaAssets.Api.Security;

/// <summary>
/// Publica el token antiforgery en la cookie <c>XSRF-TOKEN</c> (legible por JavaScript) para que Angular
/// lo devuelva en la cabecera <c>X-XSRF-TOKEN</c> (ADR-0007).
/// </summary>
internal static class XsrfTokenCookie
{
    public const string CookieName = "XSRF-TOKEN";
    public const string HeaderName = "X-XSRF-TOKEN";

    public static void Issue(HttpContext context)
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);

        context.Response.Cookies.Append(
            CookieName,
            tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false, // Debe leerlo el cliente; el token solo sirve junto con la cookie HttpOnly de antiforgery.
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Path = "/",
            });
    }
}

/// <summary>
/// Emite la cookie XSRF-TOKEN en las lecturas de la API, ya con la identidad autenticada resuelta.
/// </summary>
internal sealed class XsrfTokenMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path.StartsWithSegments("/v1"))
        {
            XsrfTokenCookie.Issue(context);
        }

        return next(context);
    }
}
