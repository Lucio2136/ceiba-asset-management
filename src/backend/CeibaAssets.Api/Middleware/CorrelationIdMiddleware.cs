using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace CeibaAssets.Api.Middleware;

/// <summary>
/// Propaga un identificador de correlación por solicitud: lo acepta del cliente si tiene un
/// formato seguro, o genera uno nuevo. Se incluye en la respuesta, en los logs y en Problem Details.
/// </summary>
internal sealed partial class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-ID";

    private const string ItemKey = "CeibaAssets.CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Resolve(context.Request.Headers[HeaderName]);
        context.Items[ItemKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await next(context);
        }
    }

    public static string? GetCorrelationId(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var value) ? value as string : null;

    private static string Resolve(StringValues headerValues)
    {
        var candidate = headerValues.Count == 1 ? headerValues[0] : null;

        return candidate is not null && AllowedFormat().IsMatch(candidate)
            ? candidate
            : Guid.NewGuid().ToString("N");
    }

    [GeneratedRegex("^[A-Za-z0-9._:-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedFormat();
}
