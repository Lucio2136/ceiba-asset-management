namespace CeibaAssets.Contracts.Common;

/// <summary>
/// Página de resultados con el total para paginación.
/// </summary>
/// <typeparam name="T">Tipo de elemento.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>Elementos de la página actual.</summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>Total de elementos que cumplen el filtro.</summary>
    public required int TotalCount { get; init; }

    /// <summary>Número de página (desde 1).</summary>
    public required int Page { get; init; }

    /// <summary>Tamaño de página aplicado.</summary>
    public required int PageSize { get; init; }
}
