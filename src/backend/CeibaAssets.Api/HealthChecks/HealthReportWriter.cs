using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CeibaAssets.Api.HealthChecks;

/// <summary>
/// Respuesta JSON de health checks sin descripciones ni excepciones, para no exponer detalles internos.
/// </summary>
internal static class HealthReportWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var payload = new
        {
            Status = report.Status.ToString(),
            TotalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                DurationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
            }),
        };

        return context.Response.WriteAsJsonAsync(payload, SerializerOptions, context.RequestAborted);
    }
}
