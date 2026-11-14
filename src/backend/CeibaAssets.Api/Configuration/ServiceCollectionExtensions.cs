using System.Text.Json.Serialization;
using CeibaAssets.Api.HealthChecks;
using CeibaAssets.Api.Middleware;
using CeibaAssets.Api.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace CeibaAssets.Api.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // AddControllersWithViews registra AutoValidateAntiforgeryTokenAuthorizationFilter
        // (AddControllers solo no lo hace; sin eso falla en tiempo de ejecución).
        services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context.HttpContext);
            if (correlationId is not null)
            {
                context.ProblemDetails.Extensions["correlationId"] = correlationId;
            }
        });

        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddApiRateLimiting();

        // Las respuestas comprimidas no deben contener secretos junto a datos controlados por el cliente (BREACH).
        services.AddResponseCompression(options => options.EnableForHttps = true);

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"]);

        services.AddOpenApiDocument(settings =>
        {
            settings.DocumentName = "v1";
            settings.Title = "CEIBA Asset Management API";
            settings.Version = "v1";
        });

        return services;
    }
}
