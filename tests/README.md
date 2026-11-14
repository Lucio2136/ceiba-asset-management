# Pruebas

| Ubicación                               | Tipo                                                      | Herramienta                          | Estado   |
| --------------------------------------- | --------------------------------------------------------- | ------------------------------------ | -------- |
| `backend/CeibaAssets.Api.Tests`         | API en memoria: salud, errores, cabeceras, correlación, rate limiting | xUnit v3, WebApplicationFactory | Fase 1 |
| `backend/CeibaAssets.Architecture.Tests` | Reglas de dependencia entre capas                         | xUnit v3, reflexión                  | Fase 1   |
| `integration/`                          | Integración con SQL Server real                           | xUnit v3                             | Fase 2   |
| `agent/`                                | Agente PowerShell con fixtures de CIM y PnP               | Pester 5 en PowerShell 7             | Fase 7   |
| `../src/frontend/ceiba-assets-web`      | Unitarias (`*.spec.ts`) y E2E (`e2e/`)                    | Vitest, Playwright                   | Fase 1   |

## Comandos

```powershell
# Backend (desde la raíz del repositorio)
dotnet test --solution CeibaAssets.slnx

# Frontend (desde src/frontend/ceiba-assets-web)
npm test        # Vitest
npm run e2e     # Playwright con Microsoft Edge
```

## Criterios

- Las pruebas son proporcionales al riesgo: los flujos críticos (alta, asignación, devolución,
  movimiento, mantenimiento, baja, descubrimiento, conciliación, sincronización y auditoría)
  tienen pruebas de API y E2E cuando se implementan.
- Las pruebas de API usan el entorno `Testing` para validar el comportamiento no productivo.
- Los E2E simulan la API; cualquier llamada no simulada hace fallar la prueba.
