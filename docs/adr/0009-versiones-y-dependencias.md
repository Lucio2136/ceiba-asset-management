# ADR-0009: Política de versiones y dependencias

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

En noviembre de 2026 ya existen .NET 11, EF Core 11 y TypeScript 7. El stack acordado es
.NET 10 LTS, Angular 22 y TypeScript 6, con la menor cantidad posible de dependencias y archivos
lock versionados.

## Decisión

### Versiones de plataforma

| Componente        | Versión fijada            | Motivo                                                                 |
| ----------------- | ------------------------- | ---------------------------------------------------------------------- |
| .NET SDK          | 10.0.404 (`global.json`, `rollForward: latestFeature`) | Línea LTS acordada                                  |
| Runtime ASP.NET Core | 10.0.15                | Último parche LTS                                                      |
| Angular           | 22.2                      | Versión mayor acordada (Angular 23 aún no es estable)                  |
| TypeScript        | 6.0.3                     | Angular 22 exige `>=6.0 <6.1`; TypeScript 7 no es compatible          |
| Node.js           | 24 LTS (`>=24.15`)        | Requisito de Angular CLI 22                                            |

### Paquetes .NET

- **Central Package Management** (`Directory.Packages.props`) con
  `CentralPackageTransitivePinningEnabled`, y **archivos lock** (`packages.lock.json`). CI
  restaurará en modo bloqueado.
- **Auditoría NuGet** activa para dependencias directas y transitivas desde severidad baja. Con
  `TreatWarningsAsErrors`, una vulnerabilidad conocida detiene la compilación.
- Los paquetes ligados al runtime se mantienen en **10.0.x**: EF Core 11 solo admite `net11.0`, y
  `Microsoft.AspNetCore.Mvc.Testing` 11 corresponde a ASP.NET Core 11.
- Los paquetes de `dotnet/extensions` se publican de forma independiente y admiten `net10.0`.
  Para ellos se usa la última versión estable: `Microsoft.Extensions.Http.Resilience` 11.0.0
  depende de paquetes de runtime 10.0.x en `net10.0`.
- Paquetes instalados en la Fase 1, aunque algunos se usarán después: EF Core SqlServer y Design
  (Fase 2), FluentValidation (registrado desde la Fase 1), NSwag, Serilog y Http.Resilience
  (Fase 9).

### Paquetes npm

- `package-lock.json` versionado; CI usará `npm ci`.
- **`@lucide/angular` en lugar de `lucide-angular`.** `lucide-angular` está marcado como obsoleto
  en npm ("Please use @lucide/angular instead") y declara compatibilidad solo hasta Angular 21.
  `@lucide/angular` es el sucesor oficial (mismo repositorio de Lucide), standalone, basado en
  signals y compatible con Angular 22.
- **Override temporal de `minimatch` (`^11.0.8`) para `@typescript-eslint/typescript-estree`.**
  typescript-eslint 8.72.2, su última versión, depende de `minimatch ^10.2.2`, afectado por
  [GHSA-g3hv-75wg-m249](https://github.com/advisories/GHSA-g3hv-75wg-m249) (denegación de
  servicio, severidad moderada, corregido en 11.0.1). ESLint 10 ya usa `minimatch` 11 con la
  misma API. El override se eliminará cuando typescript-eslint actualice su dependencia; tras
  aplicarlo, `npm audit` reporta 0 vulnerabilidades y el lint pasa.
- **`@types/node`** solo para tipar el código E2E (`playwright.config.ts` usa `process.env`), como
  en la configuración estándar de Playwright.
- No se instalan Bootstrap, Tailwind, NgRx, Moment.js, Axios, Sonner, ngx-translate, ngx-mask,
  MSW ni Angular Testing Library.

### Servidor de desarrollo y `@lucide/angular`

`@lucide/angular` se distribuye como un único módulo de 11.5 MB con todos los íconos. Sin
tree-shaking, el servidor de desarrollo entregaba entre 56 y 60 MB de JavaScript y la primera
carga superaba 30 segundos. La configuración `development` habilita
`optimization.scripts` (tree-shaking y minificación) y conserva los source maps con su
contenido. La recarga automática sigue funcionando; HMR no aplica con scripts optimizados.

## Alternativas consideradas

- **Adoptar .NET 11, EF Core 11 o TypeScript 7**: fuera del stack acordado; .NET 11 es STS.
- **Aceptar la vulnerabilidad de `minimatch` sin override**: dejaría una alerta conocida abierta
  sin necesidad.
- **Desactivar el pre-empaquetado solo para Lucide**: se probó y no basta, porque sin
  optimización Angular no descarta las clases de íconos no usadas.

## Consecuencias

- Cada actualización de dependencias revisa este ADR, el override de `minimatch` y la
  compatibilidad de peers con Angular 22.
- El código del servidor de desarrollo está minificado; la depuración usa los source maps.
