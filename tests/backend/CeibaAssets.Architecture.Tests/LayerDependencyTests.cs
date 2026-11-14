using System.Reflection;

namespace CeibaAssets.Architecture.Tests;

/// <summary>
/// Protege las reglas de dependencia del monolito modular: el dominio no conoce la infraestructura,
/// los contratos públicos no exponen el dominio y ninguna capa interna depende de la API.
/// </summary>
public sealed class LayerDependencyTests
{
    public static TheoryData<string, string> ForbiddenDependencies => new()
    {
        { "CeibaAssets.Domain", "CeibaAssets.Application" },
        { "CeibaAssets.Domain", "CeibaAssets.Infrastructure" },
        { "CeibaAssets.Domain", "CeibaAssets.Contracts" },
        { "CeibaAssets.Domain", "CeibaAssets.Api" },
        { "CeibaAssets.Domain", "Microsoft.EntityFrameworkCore" },
        { "CeibaAssets.Domain", "Microsoft.AspNetCore" },
        { "CeibaAssets.Application", "CeibaAssets.Infrastructure" },
        { "CeibaAssets.Application", "CeibaAssets.Contracts" },
        { "CeibaAssets.Application", "CeibaAssets.Api" },
        { "CeibaAssets.Application", "Microsoft.AspNetCore" },
        { "CeibaAssets.Application", "Microsoft.EntityFrameworkCore.SqlServer" },
        { "CeibaAssets.Contracts", "CeibaAssets.Domain" },
        { "CeibaAssets.Contracts", "CeibaAssets.Application" },
        { "CeibaAssets.Contracts", "CeibaAssets.Infrastructure" },
        { "CeibaAssets.Infrastructure", "CeibaAssets.Contracts" },
        { "CeibaAssets.Infrastructure", "CeibaAssets.Api" },
        { "CeibaAssets.SyncWorker", "CeibaAssets.Api" },
    };

    [Theory]
    [MemberData(nameof(ForbiddenDependencies))]
    public void Assembly_DoesNotReferenceForbiddenDependency(string assemblyName, string forbiddenAssembly)
    {
        var assembly = Assembly.Load(new AssemblyName(assemblyName));

        var violations = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .Where(name => name == forbiddenAssembly
                || name.StartsWith(forbiddenAssembly + ".", StringComparison.Ordinal));

        Assert.Empty(violations);
    }
}
