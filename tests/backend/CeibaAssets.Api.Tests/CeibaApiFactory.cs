using CeibaAssets.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace CeibaAssets.Api.Tests;

/// <summary>
/// Host de la API en memoria con un entorno distinto de Development, para validar el comportamiento productivo.
/// </summary>
public sealed class CeibaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "CeibaTests-" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Database:Provider", "InMemory");
        builder.UseSetting("Database:Name", _databaseName);
        // Satisface la validación de cadena aunque InMemory no la use.
        builder.UseSetting("ConnectionStrings:CeibaAssets", "Server=(localdb)\\unused;Database=unused");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        DatabaseInitializer.InitializeAsync(host.Services, applyMigrations: false, seedDemoData: false)
            .GetAwaiter()
            .GetResult();
        return host;
    }
}
