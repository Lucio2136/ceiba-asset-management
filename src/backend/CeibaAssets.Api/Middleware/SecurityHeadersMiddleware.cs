using Microsoft.Net.Http.Headers;

namespace CeibaAssets.Api.Middleware;

internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ApiContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(ApplyHeaders, context);
        return next(context);
    }

    private static Task ApplyHeaders(object state)
    {
        var context = (HttpContext)state;
        var headers = context.Response.Headers;

        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // Swagger UI (solo en Development) necesita scripts y estilos propios.
        if (!context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            headers.ContentSecurityPolicy = ApiContentSecurityPolicy;

            if (!headers.ContainsKey(HeaderNames.CacheControl))
            {
                headers.CacheControl = "no-store";
            }
        }

        return Task.CompletedTask;
    }
}
