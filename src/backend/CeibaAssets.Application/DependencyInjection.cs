using CeibaAssets.Application.Abstractions;
using CeibaAssets.Application.Assets;
using CeibaAssets.Application.Assignments;
using CeibaAssets.Application.Audit;
using CeibaAssets.Application.Locations;
using CeibaAssets.Application.People;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CeibaAssets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentUser, SystemCurrentUser>();

        services.AddScoped<IAuditTrail, AuditTrail>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IBuildingsService, BuildingsService>();
        services.AddScoped<ILocationsService, LocationsService>();
        services.AddScoped<IAssetsService, AssetsService>();
        services.AddScoped<IAssignmentsService, AssignmentsService>();
        services.AddScoped<IResponsiblePeopleService, ResponsiblePeopleService>();

        return services;
    }
}
