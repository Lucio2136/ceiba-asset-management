using CeibaAssets.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CeibaAssets.Api.HealthChecks;

/// <summary>
/// Verifica que la base de datos acepta conexiones (SQL Server en producción; InMemory en pruebas).
/// </summary>
internal sealed class DatabaseHealthCheck(CeibaDbContext db) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("La base de datos no responde.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("La base de datos no está disponible.", ex);
        }
    }
}
