# Despliegue

Los scripts de despliegue se entregan en la Fase 10, en `deploy/`. Este documento fija la
topología objetivo para que las fases intermedias no la contradigan.

> La aplicación se publicará en internet ([ADR-0011](../adr/0011-publicacion-en-internet.md)).
> No se ejecutan cambios sobre servidores reales sin autorización explícita.

## Topología objetivo

| Elemento               | Configuración prevista                                                        |
| ---------------------- | ----------------------------------------------------------------------------- |
| Servidor               | Windows Server 2025 accesible desde internet (proveedor pendiente)            |
| Nombre DNS             | Dominio público pendiente de definir                                          |
| Sitio IIS              | Enlace HTTPS con certificado de una autoridad pública; SPA en la raíz         |
| Aplicación `/api`      | Grupo de aplicaciones dedicado ("No Managed Code"), ASP.NET Core Module in-process |
| SyncWorker             | Windows Service con cuenta de servicio dedicada                               |
| Agente                 | Tarea programada en los equipos, con su propia identidad (Fase 7)            |
| SQL Server 2025 Express | Solo accesible desde el propio servidor; inicios de sesión separados para migraciones y ejecución |

## Prerrequisitos del servidor

- Rol de IIS con el módulo URL Rewrite.
- ASP.NET Core Hosting Bundle para .NET 10.
- Certificado TLS público con renovación automática para el dominio.
- Firewall: entrada pública solo en 443 y en 80 (redirección a HTTPS y validación del
  certificado). SQL Server, escritorio remoto y administración remota no se exponen a internet.
- Carpetas de logs con permisos NTFS limitados a las cuentas de servicio y de operaciones.

## Principios de despliegue y rollback

- Cada versión se publica en una carpeta nueva; el rollback reapunta a la versión anterior.
- `app_offline.htm` durante el cambio de binarios de la API.
- Migraciones de base de datos con scripts idempotentes generados desde EF Core, precedidas de un
  respaldo verificado.
- Verificación posterior: `/api/health/ready`, E2E de humo con `E2E_BASE_URL` y revisión de logs.
