# ADR-0003: Organización del repositorio y límites entre proyectos

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

El repositorio contiene backend .NET, frontend Angular, un agente PowerShell, scripts de base de
datos, scripts de despliegue y documentación. Se necesita una estructura que haga evidentes las
responsabilidades y que impida dependencias indebidas entre capas.

## Decisión

### Estructura

Un solo repositorio con la estructura documentada en el [README](../../README.md). La solución
`CeibaAssets.slnx` (formato SLNX de .NET 10) vive en la raíz e incluye los proyectos de
`src/backend` y `tests/backend`. La configuración común de MSBuild está en
`Directory.Build.props` y las versiones de paquetes en `Directory.Packages.props`.

### Capas del backend

| Proyecto                     | Responsabilidad                                                      | Puede depender de                        |
| ---------------------------- | -------------------------------------------------------------------- | ---------------------------------------- |
| `CeibaAssets.Domain`         | Entidades, value objects, reglas e invariantes                       | Nada                                     |
| `CeibaAssets.Application`    | Casos de uso, interfaces, validación, DTO internos, autorización     | Domain                                   |
| `CeibaAssets.Infrastructure` | EF Core, SQL Server, Identity, integraciones, servicios externos     | Application, Domain                      |
| `CeibaAssets.Contracts`      | Contratos públicos de la API                                         | Nada                                     |
| `CeibaAssets.Api`            | Controladores, autenticación, OpenAPI, Problem Details, middleware   | Application, Infrastructure, Contracts   |
| `CeibaAssets.SyncWorker`     | Sincronización con InvGate, reintentos, snapshots                    | Application, Infrastructure              |

- La API traduce entre `Contracts` (público) y los DTO internos de `Application`, sin
  AutoMapper.
- No se crea un repositorio genérico que envuelva `DbSet`. Los repositorios específicos solo
  existen cuando una operación de dominio o una consulta lo justifica.
- `tests/backend/CeibaAssets.Architecture.Tests` verifica estas reglas sobre las referencias
  reales de cada ensamblado.

### Frontend

- Organización por funcionalidad: `core/` (servicios y estructura únicos de la aplicación),
  `shared/` (piezas reutilizables con responsabilidad clara) y `features/` (verticales
  funcionales). Las carpetas de `features/` se crean al implementar cada vertical.
- Nombres de archivo con sufijo de tipo (estilo 2016 de Angular: `asset-table.component.ts`,
  clase `AssetTableComponent`), coherente con los componentes previstos del proyecto.
- Las pruebas del frontend viven dentro del workspace Angular: pruebas unitarias junto al código
  (`*.spec.ts`) y E2E en `e2e/`. Las herramientas (builder de Angular y Playwright) resuelven
  dependencias desde ese workspace; `tests/frontend/` solo apunta a esa ubicación.

### Ramas y commits

- `main` es la rama integrada. Cada fase se trabaja en `feature/phase-NN-<tema>` y se integra
  mediante pull request.
- Mensajes con prefijos de Conventional Commits (`feat`, `fix`, `chore`, `docs`, `test`) y
  descripción en español.

## Alternativas consideradas

- **Repositorios separados por componente**: complican cambios coordinados entre contrato,
  backend y frontend, que en este proyecto son frecuentes.
- **Pruebas del frontend en `tests/frontend`**: requeriría un segundo `package.json` o resolver
  módulos fuera del workspace, sin beneficio funcional.

## Consecuencias

- Las referencias prohibidas fallan en las pruebas, no en la revisión manual.
- Mapear entre contratos públicos y DTO internos agrega código explícito, a cambio de poder
  evolucionar la API pública sin acoplarla al modelo interno.
