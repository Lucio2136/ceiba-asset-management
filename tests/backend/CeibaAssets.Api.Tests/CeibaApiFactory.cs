using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CeibaAssets.Api.Tests;

/// <summary>
/// Host de la API en memoria con un entorno distinto de Development, para validar el comportamiento productivo.
/// </summary>
public sealed class CeibaApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");
}
