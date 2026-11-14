# Convenciones de desarrollo

## Idioma

- Código, identificadores y nombres de archivo en inglés.
- Interfaz de usuario, documentación, ADR y mensajes de commit en español (es-MX).

## API

- Rutas públicas: `/api/v1/<recurso>` con recursos en plural y kebab-case. En código se declaran
  sin `api` ([ADR-0005](../adr/0005-controladores-y-rutas-versionadas.md)).
- JSON en camelCase (System.Text.Json).
- Fechas y horas en UTC con formato ISO 8601; la conversión a hora local ocurre en la SPA.
- Listas: `{ items, totalCount, page, pageSize }`, con paginación, filtros y orden en SQL.
- Errores: Problem Details con `traceId` y `correlationId`; códigos HTTP correctos (400, 401,
  403, 404, 409, 422, 429, 500).
- Contratos: `sealed record` en `CeibaAssets.Contracts`, con propiedades `required` e `init` para
  los campos obligatorios y documentación XML (alimenta OpenAPI).
- Cada endpoint declara sus respuestas con `[ProducesResponseType]`.

## .NET

- Espacios de nombres por archivo, `var` cuando el tipo es evidente, llaves obligatorias.
- Campos privados `_camelCase`; constantes y campos `static readonly` en PascalCase; interfaces con
  prefijo `I`.
- Métodos asíncronos con `CancellationToken` de punta a punta.
- Lecturas con `AsNoTracking` y proyección directa a DTO; sin N+1; sin devolver entidades.
- Validación de entrada con FluentValidation en `Application`; la validación del frontend no
  sustituye a la del backend.
- Configuración tipada con `IOptions<T>`, validada con DataAnnotations y `ValidateOnStart`.
- Sin repositorio genérico, MediatR, AutoMapper ni Dapper. Dapper solo tras medir un problema
  real en una consulta concreta.

## Angular

- Componentes standalone con prefijo `ceiba`, `ChangeDetectionStrategy.OnPush` y archivos con
  sufijo de tipo (`*.component.ts`, `*.service.ts`).
- Rutas lazy-loaded por funcionalidad; `@defer` para widgets pesados como las gráficas.
- Signals para estado local y derivado; `rxResource` para cargar datos desde observables; RxJS
  para debounce, cancelación y composición.
- Los componentes no importan `HttpClient` (regla de ESLint). Usan servicios o facades que
  consumen el cliente generado por NSwag.
- No se escriben a mano DTO que ya existan en el contrato OpenAPI.
- `@for` siempre con `track` por identificador.
- Accesibilidad: reglas de plantilla de angular-eslint, encabezados jerárquicos, `aria-label` en
  botones de solo ícono y enlace para saltar al contenido.
- Íconos: componentes de `@lucide/angular` (por ejemplo, `<svg lucideHouse></svg>`).
- Estilos: tokens de Material (`var(--mat-sys-*)`) en lugar de colores fijos.

## Git

- Ramas `feature/phase-NN-<tema>` sobre `main`, integradas por pull request.
- Commits con prefijo de Conventional Commits y descripción en español.
- No se versionan secretos, certificados, llaves, respaldos ni bases de datos locales (ver
  `.gitignore`).
