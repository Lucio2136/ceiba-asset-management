# Entorno de desarrollo

## Herramientas validadas en la Fase 1

| Herramienta        | Versión detectada                  | Notas                                                                 |
| ------------------ | ---------------------------------- | --------------------------------------------------------------------- |
| Windows 11         | Build 26300                        | Equipo de desarrollo                                                  |
| Node.js            | 24.16.0                            | Angular CLI 22 requiere `^22.22.3`, `^24.15.0` o `>=26`               |
| npm                | 11.13.0                            |                                                                       |
| Git                | 2.55.0                             |                                                                       |
| .NET SDK           | 10.0.404                           | Instalado por usuario en `%LOCALAPPDATA%\Microsoft\dotnet`            |
| Runtimes .NET      | Microsoft.NETCore.App / AspNetCore.App 10.0.15 |                                                           |
| Windows PowerShell | 5.1 (incluye Pester 3.4.0)         | El agente requiere PowerShell 7 (Fase 7)                              |
| Microsoft Edge     | Instalado                          | Navegador de los E2E (`channel: 'msedge'`)                            |

### Instalación del SDK de .NET

El SDK se instaló sin privilegios de administrador con el script oficial
`dotnet-install.ps1` (firma Authenticode de Microsoft verificada antes de ejecutarlo):

```powershell
.\dotnet-install.ps1 -Channel 10.0 -InstallDir "$env:LOCALAPPDATA\Microsoft\dotnet" -NoPath
```

Después se agregaron, a nivel de usuario, la carpeta de instalación al `PATH` y la variable
`DOTNET_ROOT`. Las terminales abiertas antes de ese cambio deben reiniciarse.

### Pendientes para fases posteriores

| Herramienta       | Fase | Motivo                                                                 |
| ----------------- | ---- | ---------------------------------------------------------------------- |
| SQL Server 2025 (Developer o Express) | 2 | Base de datos local; su instalación requiere permisos de administrador |
| PowerShell 7 y Pester 5 | 7 | Agente y sus pruebas                                                  |
| GitHub CLI (opcional) | — | Crear pull requests desde la terminal                                |
| IIS y Hosting Bundle  | 10 | Servidor de laboratorio Windows Server 2025                           |

## Primer uso

```powershell
git clone <url-del-repositorio>
cd ceiba-asset-management

# Backend
dotnet restore
dotnet build
dotnet test --solution CeibaAssets.slnx

# Frontend (también instala los hooks de Husky)
cd src/frontend/ceiba-assets-web
npm ci
```

Confiar en el certificado de desarrollo de ASP.NET Core (Windows pide confirmación):

```powershell
dotnet dev-certs https --trust
```

## Ejecución local

| Proceso | Comando                                                                 | URL                                  |
| ------- | ----------------------------------------------------------------------- | ------------------------------------ |
| API     | `dotnet run --project src/backend/CeibaAssets.Api --launch-profile https` | `https://localhost:7135/api`         |
| Swagger | (con la API en ejecución)                                               | `https://localhost:7135/api/swagger` |
| SPA     | `npm start` en `src/frontend/ceiba-assets-web`                          | `http://localhost:4200`              |

El servidor de Angular redirige `/api` a la API ([ADR-0006](../adr/0006-spa-y-api-mismo-origen-iis.md)).

## Notas

- La configuración `development` del frontend minifica los scripts para aplicar tree-shaking a
  `@lucide/angular` ([ADR-0009](../adr/0009-versiones-y-dependencias.md)). La depuración usa los
  source maps.
- npm puede mostrar `Unknown env config "devdir"`: proviene de la configuración de npm del entorno
  y no afecta al proyecto.
- Si `dotnet` no se reconoce en una terminal, reiníciala para que tome el `PATH` actualizado.
