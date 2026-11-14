/*
 * DTOs TEMPORALES comunes. Se reemplazan por los generados por NSwag cuando existan.
 */

/** Respuesta paginada estándar de la API (docs/architecture/conventions.md). */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type SortDirection = 'asc' | 'desc';
