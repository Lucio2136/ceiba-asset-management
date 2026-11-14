using CeibaAssets.Application.Security;
using CeibaAssets.Infrastructure.Seeding;

namespace CeibaAssets.Api.Configuration;

internal static class DatabaseInitializationExtensions
{
    /// <summary>
    /// Solo en Development: aplica migraciones, crea roles y datos ficticios, y aprovisiona el administrador
    /// de laboratorio desde <c>Seed:AdminEmail</c> y <c>Seed:AdminPassword</c> (user-secrets).
    /// Otros entornos aprovisionan con procedimientos explícitos; Testing lo hace la fábrica de pruebas.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CeibaAssets.DatabaseInitialization");

        try
        {
            await DatabaseInitializer.InitializeAsync(app.Services, applyMigrations: true, seedDemoData: true);

            var email = app.Configuration["Seed:AdminEmail"];
            var password = app.Configuration["Seed:AdminPassword"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning(
                    "No se creó el administrador de laboratorio: defina Seed:AdminEmail y Seed:AdminPassword (user-secrets).");
                return;
            }

            await DatabaseInitializer.EnsureUserAsync(
                app.Services,
                email,
                password,
                "Administrador de laboratorio",
                [AppRoles.Administrator]);
            logger.LogInformation("Administrador de laboratorio disponible: {Email}.", email);
        }
        catch (Exception ex)
        {
            // La API sigue arrancando: /health/ready reportará el problema de base de datos.
            logger.LogError(
                ex,
                @"No se pudo inicializar la base de datos. Verifique SQL Server (.\SQLEXPRESS) y la cadena ConnectionStrings:CeibaAssets.");
        }
    }
}
