using CeibaAssets.Application.Abstractions;
using CeibaAssets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CeibaAssets.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "CeibaAssets";

    /// <summary>
    /// Cadena de conexión de laboratorio (SQL Server Express local). Solo la usan las herramientas de EF Core.
    /// </summary>
    internal const string DefaultDevelopmentConnectionString =
        @"Server=.\SQLEXPRESS;Database=CeibaAssets;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    /// <summary>
    /// Registra la persistencia. En pruebas use <c>Database:Provider=InMemory</c>;
    /// en el resto de entornos, <c>ConnectionStrings:CeibaAssets</c>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "SqlServer";

        services.AddDbContext<CeibaDbContext>(options =>
        {
            if (string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
            {
                var name = configuration["Database:Name"] ?? "CeibaAssets";
                options.UseInMemoryDatabase(name);
                return;
            }

            var connectionString = configuration.GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Falta la cadena de conexión 'ConnectionStrings:{ConnectionStringName}'. " +
                    "Defínala en appsettings, en user-secrets o con la variable ConnectionStrings__CeibaAssets.");

            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(CeibaDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<CeibaDbContext>());

        return services;
    }
}
