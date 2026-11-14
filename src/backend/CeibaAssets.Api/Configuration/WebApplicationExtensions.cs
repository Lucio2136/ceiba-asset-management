using CeibaAssets.Api.HealthChecks;
using CeibaAssets.Api.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace CeibaAssets.Api.Configuration;

internal static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        var pathBase = app.Configuration["Hosting:PathBase"];
        if (!string.IsNullOrWhiteSpace(pathBase))
        {
            app.UsePathBase(pathBase);
        }

        // Registran sus cabeceras en OnStarting para sobrevivir al reinicio de la respuesta
        // que hace el manejador de excepciones.
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<SecurityHeadersMiddleware>();

        // Sin un logger explícito, el middleware escribe en el Log estático, que se preserva vacío.
        app.UseSerilogRequestLogging(
            options => options.Logger = app.Services.GetRequiredService<Serilog.ILogger>());
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseResponseCompression();

        if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthorization();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthReportWriter.WriteAsync,
        }).DisableRateLimiting();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthReportWriter.WriteAsync,
        }).DisableRateLimiting();

        return app;
    }
}
