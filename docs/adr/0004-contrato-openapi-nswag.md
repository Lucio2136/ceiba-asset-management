# ADR-0004: OpenAPI como contrato y NSwag como generador

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

- La SPA no debe duplicar a mano los modelos del backend.
- Se requiere Swagger UI para desarrollo y un cliente TypeScript para Angular.
- La plantilla `webapi` de .NET 10 incluye `Microsoft.AspNetCore.OpenApi`; usarlo junto con NSwag
  o Swashbuckle implicaría dos generadores con resultados distintos.

## Decisión

- **NSwag.AspNetCore** registra el documento OpenAPI `v1` y expone Swagger UI solo cuando
  `OpenApi:Enabled` es `true` (habilitado en `appsettings.Development.json`).
- **NSwag.MSBuild** ejecuta `nswag.json` después de cada compilación Debug de la API, solo cuando
  cambian la API, sus contratos o la configuración. Genera dos artefactos versionados:
  - `src/backend/CeibaAssets.Api/OpenApi/ceiba-assets-api.v1.json` (contrato revisable en PR).
  - `src/frontend/ceiba-assets-web/src/app/core/http/generated/ceiba-api.client.ts` (cliente
    Angular).
- El proyecto se creó con `--no-openapi`: no se usa `Microsoft.AspNetCore.OpenApi` ni Swashbuckle.
- Configuración del cliente: plantilla Angular, servicios `providedIn: 'root'`, `InjectionToken`
  `API_BASE_URL`, DTO como interfaces, fechas como cadenas ISO 8601 y enumeraciones como uniones
  de literales de texto.
- Los contratos son `sealed record` con propiedades `required` e `init`. Así el esquema declara
  los campos obligatorios y TypeScript no los marca como opcionales.
- El código generado se excluye de ESLint y Prettier y se marca como generado en
  `.gitattributes`.

## Alternativas consideradas

- **Swashbuckle**: no genera clientes TypeScript; requeriría otra herramienta.
- **Microsoft.AspNetCore.OpenApi + generador externo**: dos piezas para lo que NSwag resuelve con
  una.
- **Generar el cliente solo en CI**: los desarrolladores trabajarían con un cliente desfasado.

## Consecuencias

- Toda modificación de un contrato produce un diff visible en el JSON y en el cliente.
- CI (Fase 11) regenerará los artefactos y fallará si hay diferencias sin confirmar.
- La generación agrega unos 11 segundos a la compilación Debug cuando cambia la API. Se omite
  con `-p:GenerateOpenApiArtifacts=false` y no se ejecuta en compilaciones Release.
