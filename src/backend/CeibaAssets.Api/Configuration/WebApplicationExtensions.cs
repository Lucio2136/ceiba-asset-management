using CeibaAssets.Api.HealthChecks;
using CeibaAssets.Api.Middleware;
using CeibaAssets.Api.Security;
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

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler();
        }

        app.UseStatusCodePages();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        // El test server de WebApplicationFactory usa HTTP; la redirección HTTPS rompe las pruebas.
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.UseResponseCompression();

        if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
        {
            app.UseOpenApi();
            app.UseSwaggerUi();
        }

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<XsrfTokenMiddleware>();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapControllers();

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = HealthReportWriter.WriteAsync,
        }).DisableRateLimiting().AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthReportWriter.WriteAsync,
        }).DisableRateLimiting().AllowAnonymous();

        return app;
    }
}
