namespace CeibaAssets.Application.Common;

public sealed record PagedModel<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Normaliza los parámetros de paginación recibidos.
/// </summary>
public static class Paging
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (Math.Max(page, 1), pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize));
}
