using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CeibaAssets.Infrastructure.Persistence;

/// <summary>
/// Fábrica para las herramientas de EF Core (<c>dotnet ef</c>); no se conecta a la base de datos
/// al generar migraciones.
/// </summary>
public sealed class CeibaDbContextFactory : IDesignTimeDbContextFactory<CeibaDbContext>
{
    public CeibaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__CeibaAssets")
            ?? DependencyInjection.DefaultDevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<CeibaDbContext>()
            .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(CeibaDbContext).Assembly.FullName))
            .Options;

        return new CeibaDbContext(options);
    }
}
