# Despliegue en Windows Server

Scripts y plantillas de la Fase 10. La topología objetivo está en
[docs/deployment](../docs/deployment/README.md).

| Carpeta              | Contenido                                                                  |
| -------------------- | -------------------------------------------------------------------------- |
| `iis/`               | Sitio, aplicación `/api`, grupos de aplicaciones, bindings HTTPS, `web.config` de la SPA |
| `sql-server/`        | Configuración de la instancia, inicios de sesión, acceso restringido      |
| `windows-services/`  | Instalación y cuenta del servicio `CeibaAssets.SyncWorker`                 |
| `scheduled-tasks/`   | Registro de la tarea programada del agente                                 |
| `certificates/`      | Procedimientos para solicitar e instalar certificados                      |

## Reglas

- Los scripts son idempotentes, parametrizados y admiten `-WhatIf`.
- No se versionan certificados, llaves privadas ni secretos; `certificates/` contiene solo
  procedimientos.
- No se ejecutan sobre servidores reales sin autorización explícita.
