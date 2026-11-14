namespace CeibaAssets.Application.Common;

/// <summary>
/// Construye patrones para <c>EF.Functions.Like</c> a partir del texto de búsqueda del usuario.
/// </summary>
internal static class SearchPattern
{
    /// <summary>
    /// Devuelve <c>%texto%</c> sin comodines de LIKE controlados por el usuario.
    /// </summary>
    public static string Contains(string search)
    {
        var sanitized = new string(search.Trim().Where(c => c is not ('%' or '_' or '[' or ']')).ToArray());
        return $"%{sanitized}%";
    }
}
