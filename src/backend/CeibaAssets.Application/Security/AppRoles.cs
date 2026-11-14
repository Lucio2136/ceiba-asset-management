namespace CeibaAssets.Application.Security;

/// <summary>
/// Roles de la aplicación (ADR-0007).
/// </summary>
public static class AppRoles
{
    public const string Administrator = "Administrator";
    public const string AssetManager = "AssetManager";
    public const string Auditor = "Auditor";
    public const string Viewer = "Viewer";

    public static IReadOnlyList<string> All { get; } = [Administrator, AssetManager, Auditor, Viewer];
}
