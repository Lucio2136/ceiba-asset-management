namespace CeibaAssets.Api.Security;

/// <summary>
/// Nombres de políticas de autorización. Lectura: cualquier usuario autenticado (política de reserva).
/// </summary>
internal static class Policies
{
    /// <summary>Operaciones que modifican inventario: Administrator y AssetManager.</summary>
    public const string Write = "CanWrite";

    /// <summary>Consulta de la bitácora de auditoría: Administrator y Auditor.</summary>
    public const string Audit = "CanAudit";

    /// <summary>Límite de intentos para el inicio de sesión.</summary>
    public const string LoginRateLimit = "login";
}
