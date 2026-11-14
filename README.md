# CEIBA Asset Management

Plataforma empresarial para administrar, descubrir, conciliar, asignar y auditar los activos
tecnológicos de tres edificios ficticios:

| Edificio        | Niveles |
| --------------- | ------- |
| CEIBA Torre A   | 8       |
| CEIBA Torre B   | 10      |
| CEIBA Ecommerce | 1       |

> Todos los datos del proyecto son ficticios. No se utilizan datos, credenciales, nombres,
> direcciones ni infraestructura de ninguna organización real.

## Estado

**Fase 1 completada: fundamentos.** Hay estructura, toolchain, calidad, documentación y un flujo
técnico de extremo a extremo (SPA → API → cliente generado). Todavía no hay módulos de negocio.
La hoja de ruta completa está en [docs/architecture/overview.md](docs/architecture/overview.md#hoja-de-ruta).

## Arquitectura en breve

Monolito modular ([ADR-0002](docs/adr/0002-monolito-modular-windows-iis.md)):

- **SPA Angular 22** servida por IIS como archivos estáticos.
- **ASP.NET Core Web API (.NET 10 LTS)** en el mismo sitio, bajo `/api`.
- **SQL Server 2025** con un esquema por módulo.
- **CeibaAssets.SyncWorker**, Windows Service para la sincronización de solo lectura con InvGate.
- **Agente PowerShell 7** para el descubrimiento en equipos Windows.

Sin contenedores ni microservicios. La aplicación se publicará en internet
([ADR-0011](docs/adr/0011-publicacion-en-internet.md)). Ver [arquitectura](docs/architecture/overview.md)
y [diagramas](docs/diagrams/architecture.md).

## Estructura del repositorio

```text
├── CeibaAssets.slnx            Solución .NET (backend y pruebas)
├── Directory.Build.props       Configuración común de compilación y calidad
├── Directory.Packages.props    Versiones centralizadas de paquetes NuGet
├── global.json                 SDK de .NET y runner de pruebas
├── docs/                       Arquitectura, ADR, diagramas, seguridad, despliegue, runbooks
├── src/
│   ├── backend/                Api, Application, Domain, Infrastructure, Contracts, SyncWorker
│   ├── frontend/ceiba-assets-web/  SPA Angular
│   └── agent/CeibaAssets.Agent/    Agente PowerShell (Fase 7)
├── database/                   Scripts, seed ficticio y mantenimiento
├── tests/                      Pruebas .NET, integración y agente
├── deploy/                     Scripts de despliegue en Windows Server (Fase 10)
└── .github/workflows/          CI/CD (Fase 11)
```

## Requisitos de desarrollo

| Herramienta     | Versión                    | Uso                                      |
| --------------- | -------------------------- | ---------------------------------------- |
| .NET SDK        | 10.0.404 o superior en 10.0 | Backend (fijado en `global.json`)        |
| Node.js         | 24 LTS (`>=24.15`)         | Frontend                                 |
| npm             | 11                         | Frontend                                 |
| Git             | 2.9 o superior             | Control de versiones y hooks (`core.hooksPath`) |
| Microsoft Edge  | Actual                     | Pruebas E2E                              |
| SQL Server 2025 | Express                    | Desde la Fase 2                          |
| PowerShell      | 7.x                        | Desde la Fase 7 (agente y Pester)        |

Detalles y configuración inicial en
[docs/architecture/development-environment.md](docs/architecture/development-environment.md).

## Uso rápido

```powershell
# Backend: compilación (genera el contrato OpenAPI y el cliente Angular en Debug) y pruebas
dotnet build CeibaAssets.slnx
dotnet test --solution CeibaAssets.slnx

# API en https://localhost:7135/api (Swagger en /api/swagger)
dotnet run --project src/backend/CeibaAssets.Api --launch-profile https

# Frontend en http://localhost:4200
cd src/frontend/ceiba-assets-web
npm ci
npm start
```

## Verificación antes de integrar

```powershell
# Desde la raíz
dotnet format CeibaAssets.slnx --verify-no-changes
dotnet build CeibaAssets.slnx
dotnet test --solution CeibaAssets.slnx

# Desde src/frontend/ceiba-assets-web
npm run verify
npm run e2e
```

El hook `pre-commit` (Husky) ejecuta controles rápidos: ESLint y Prettier sobre los archivos del
frontend en el commit, y `dotnet format whitespace` sobre los archivos C# en el commit.

## Documentación

- [Arquitectura](docs/architecture/overview.md) y [convenciones](docs/architecture/conventions.md)
- [Decisiones de arquitectura (ADR)](docs/adr/README.md)
- [Línea base de seguridad](docs/security/security-baseline.md)
- [Despliegue](docs/deployment/README.md) y [runbooks](docs/runbooks/README.md)
- [Estrategia de pruebas](tests/README.md)

## Licencia

[MIT](LICENSE).
