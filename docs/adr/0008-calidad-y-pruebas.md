# ADR-0008: Herramientas de calidad y estrategia de pruebas

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

El proyecto exige no avanzar con compilación, lint o pruebas fallando, y controles locales
rápidos que no reemplacen a CI.

## Decisión

### Backend

- `TreatWarningsAsErrors`, `AnalysisLevel=latest-recommended` y `EnforceCodeStyleInBuild`. Las
  reglas de estilo y nombres viven en `.editorconfig`; el formato se verifica con
  `dotnet format --verify-no-changes`.
- **xUnit v3** ejecutado con **Microsoft Testing Platform**, habilitado en `global.json`. El SDK
  de .NET 10 ya no admite ejecutar pruebas MTP mediante VSTest, por lo que no se usan
  `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` ni `coverlet.collector`.
- Pruebas de API con `WebApplicationFactory` en un entorno `Testing`, para validar el
  comportamiento no productivo (por ejemplo, que Swagger no esté expuesto).
- Pruebas de arquitectura por reflexión sobre las referencias de cada ensamblado (ADR-0003).
- Aserciones con la API de xUnit y dobles de prueba propios; no se agregan FluentAssertions ni Moq.
- La cobertura se configurará en la Fase 11 con una extensión de cobertura para Microsoft Testing
  Platform y SonarQube.

### Frontend

- **ESLint** (configuración flat de angular-eslint y typescript-eslint, con
  `eslint-config-prettier` al final). Reglas adicionales: prefijo `ceiba`, OnPush obligatorio,
  `prefer-signals`, control flow de plantillas, reglas de accesibilidad y prohibición de importar
  `HttpClient` en componentes.
- **Prettier** como único formateador del frontend.
- **Vitest** mediante el builder `@angular/build:unit-test` con jsdom.
- **Playwright** con el Microsoft Edge instalado (`channel: 'msedge'`), sin descargar navegadores.
  Por defecto se ejecuta contra la compilación de producción servida por `ng serve`; con
  `E2E_BASE_URL` puede apuntar a un entorno desplegado. La API se simula, y cualquier llamada no
  simulada hace fallar la prueba.

### Controles locales

- **Husky** con un hook `pre-commit` que ejecuta **lint-staged** (ESLint y Prettier sobre los
  archivos del frontend en el commit) y `dotnet format whitespace` sobre los archivos C#
  incluidos en el commit.
- La compilación completa, las pruebas y los E2E se ejecutan con `npm run verify`,
  `npm run e2e` y `dotnet test`, y en CI. El hook no ejecuta E2E.

### SonarQube

Requiere un servidor y un token que aún no existen. Su ausencia no bloquea el desarrollo local; se
integrará en la Fase 11.

## Alternativas consideradas

- **Karma**: el runner por defecto de Angular 22 es Vitest.
- **Ejecutar las pruebas completas en el hook**: haría lentos los commits y desalentaría su uso.

## Consecuencias

- Una advertencia nueva del compilador o de un analizador detiene la compilación.
- Los E2E validan los mismos bundles que se publicarán en IIS.
