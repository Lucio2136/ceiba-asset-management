using CeibaAssets.Application.Security;
using CeibaAssets.Infrastructure.Identity;
using CeibaAssets.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CeibaAssets.Infrastructure.Seeding;

/// <summary>
/// Inicializa la base de datos: esquema (migraciones o <c>EnsureCreated</c>), roles y datos de demostración.
/// </summary>
public static class DatabaseInitializer
{
    /// <param name="services">Proveedor raíz de servicios.</param>
    /// <param name="applyMigrations"><see langword="true"/> aplica migraciones (SQL Server); <see langword="false"/> usa EnsureCreated (InMemory).</param>
    /// <param name="seedDemoData">Carga los datos ficticios si la base está vacía.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public static async Task InitializeAsync(
        IServiceProvider services,
        bool applyMigrations,
        bool seedDemoData,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CeibaDbContext>();

        if (applyMigrations)
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await EnsureRolesAsync(scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>());

        if (seedDemoData)
        {
            await DemoDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<TimeProvider>(), cancellationToken);
        }
    }

    /// <summary>
    /// Crea la cuenta si no existe y le asigna los roles indicados. El secreto no se registra en logs.
    /// </summary>
    public static async Task EnsureUserAsync(
        IServiceProvider services,
        string email,
        string password,
        string displayName,
        IEnumerable<string> roles)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.CreateVersion7(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName,
            };

            var created = await users.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(
                    "No se pudo crear la cuenta inicial: " + string.Join(", ", created.Errors.Select(e => e.Code)));
            }
        }

        foreach (var role in roles)
        {
            if (!await users.IsInRoleAsync(user, role))
            {
                await users.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task EnsureRolesAsync(RoleManager<ApplicationRole> roles)
    {
        foreach (var role in AppRoles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new ApplicationRole(role));
            }
        }
    }
}
