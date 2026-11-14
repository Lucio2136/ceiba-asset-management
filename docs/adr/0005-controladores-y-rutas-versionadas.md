# ADR-0005: Controladores ASP.NET Core y rutas versionadas

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

ASP.NET Core ofrece controladores y Minimal APIs. La API tendrá decenas de operaciones, se
protegerá con cookies y antiforgery (ADR-0007), generará clientes con NSwag (ADR-0004) y será
consumida por un agente instalado en muchos equipos que no se actualizan al mismo tiempo.

## Decisión

- Usaremos **controladores con `[ApiController]`**: soporte maduro en NSwag, filtros globales
  (por ejemplo, validación automática de antiforgery en la Fase 3), respuestas 400 con Problem
  Details y convenciones de enrutamiento.
- Las rutas se declaran como `v1/<recurso>` sin el prefijo `api`. El prefijo `/api` lo aporta IIS
  al hospedar la API como aplicación hija (ADR-0006); en desarrollo lo simula
  `Hosting:PathBase`. La ruta pública resultante es `/api/v1/<recurso>`.
- El versionado es por segmento de URL. Un cambio incompatible crea `v2` y mantiene `v1` mientras
  existan clientes, en especial versiones del agente.
- Convenciones de respuesta:
  - Listas: `{ items, totalCount, page, pageSize }` con paginación en el servidor.
  - Errores: Problem Details (`application/problem+json`) con `traceId` y `correlationId`.
  - JSON en camelCase y fechas en UTC con formato ISO 8601.

## Alternativas consideradas

- **Minimal APIs**: viables, pero requieren más configuración explícita para metadatos de
  OpenAPI y filtros transversales; no aportan una ventaja relevante para este proyecto.
- **Paquete Asp.Versioning**: agrega una dependencia para un esquema de versionado que se cubre
  con rutas explícitas.

## Consecuencias

- Las clases de controlador deben ser públicas para el descubrimiento de MVC.
- Cada recurso nuevo sigue la convención de ruta y los contratos de lista y error.
