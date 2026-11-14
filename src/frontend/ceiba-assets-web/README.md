# ceiba-assets-web

SPA de CEIBA Asset Management: Angular 22, Angular Material, componentes standalone, signals y
detección de cambios zoneless.

## Comandos

| Comando                 | Descripción                                                        |
| ----------------------- | ------------------------------------------------------------------ |
| `npm ci`                | Instala dependencias exactas y los hooks de Husky                  |
| `npm start`             | Servidor de desarrollo en `http://localhost:4200` con proxy `/api` |
| `npm run build`         | Compilación de producción en `dist/ceiba-assets-web/browser`       |
| `npm test`              | Pruebas unitarias con Vitest                                       |
| `npm run lint`          | ESLint (TypeScript, plantillas y accesibilidad)                    |
| `npm run format`        | Prettier                                                           |
| `npm run typecheck:e2e` | Verificación de tipos del código E2E                               |
| `npm run e2e`           | Playwright con Microsoft Edge contra la compilación de producción  |
| `npm run verify`        | Formato, lint, tipos E2E, pruebas unitarias y compilación          |

Para ejecutar los E2E contra un entorno desplegado: `$env:E2E_BASE_URL = 'https://...'; npm run e2e`.

## Estructura

```text
src/app/
├── core/
│   ├── configuration/   Proveedores de la API (ruta base /api)
│   ├── http/generated/  Cliente generado por NSwag (no editar)
│   ├── layout/          Shell, inicio y página no encontrada
│   └── services/        Servicios únicos de la aplicación
├── shared/              Piezas reutilizables (se agregan cuando existe una necesidad clara)
├── features/            Verticales funcionales con rutas lazy-loaded
└── app.routes.ts
```

## Reglas

- El cliente HTTP y sus DTO se generan desde OpenAPI al compilar la API en Debug. No se editan ni
  se duplican a mano.
- Los componentes no usan `HttpClient`; consumen servicios o facades.
- Componentes con prefijo `ceiba`, OnPush y sufijo de tipo en archivos y clases.
- Íconos con `@lucide/angular` (`<svg lucideHouse></svg>`).
- Estilos con tokens de Material (`var(--mat-sys-*)`).

Más detalles en [docs/architecture/conventions.md](../../../docs/architecture/conventions.md).
