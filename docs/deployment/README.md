# Despliegue

Los scripts de despliegue se entregan en la Fase 10, en `deploy/`. Este documento fija la
topología objetivo para que las fases intermedias no la contradigan.

> No se ejecutan cambios sobre servidores reales sin autorización explícita.

## Topología objetivo (laboratorio)

| Elemento               | Configuración prevista                                                        |
| ---------------------- | ----------------------------------------------------------------------------- |
| Servidor               | Windows Server 2025                                                           |
| Nombre DNS (ficticio)  | `assets.ceiba.internal` (`.internal` está reservado para redes privadas)      |
| Sitio IIS              | Enlace HTTPS con certificado TLS; SPA en la raíz                              |
| Aplicación `/api`      | Grupo de aplicaciones dedicado ("No Managed Code"), ASP.NET Core Module in-process |
| SyncWorker             | Windows Service con cuenta de servicio dedicada                               |
| Agente                 | Tarea programada en los equipos, con su propia identidad (Fase 7)            |
| SQL Server 2025        | Inicios de sesión separados para migraciones y ejecución                     |

## Prerrequisitos del servidor

- Rol de IIS con el módulo URL Rewrite.
- ASP.NET Core Hosting Bundle para .NET 10.
- Certificado TLS emitido para el nombre DNS.
- Reglas de firewall: entrada solo en 443 hacia IIS y en el puerto de SQL Server desde los
  servidores autorizados.
- Carpetas de logs con permisos NTFS limitados a las cuentas de servicio y de operaciones.

## Principios de despliegue y rollback

- Cada versión se publica en una carpeta nueva; el rollback reapunta a la versión anterior.
- `app_offline.htm` durante el cambio de binarios de la API.
- Migraciones de base de datos con scripts idempotentes generados desde EF Core, precedidas de un
  respaldo verificado.
- Verificación posterior: `/api/health/ready`, E2E de humo con `E2E_BASE_URL` y revisión de logs.
