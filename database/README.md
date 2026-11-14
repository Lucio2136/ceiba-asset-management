# Base de datos

SQL Server 2025. El modelo se implementa a partir de la Fase 2.

| Carpeta        | Contenido                                                                          |
| -------------- | ---------------------------------------------------------------------------------- |
| `scripts/`     | T-SQL idempotente que no pertenece a las migraciones: inicios de sesión, usuarios, roles y permisos de mínimo privilegio |
| `seed/`        | Datos exclusivamente ficticios (edificios, pisos, ubicaciones, catálogos)          |
| `maintenance/` | Índices y estadísticas, verificación de integridad, respaldos y restauración       |

## Convenciones

- Un esquema por módulo: `location`, `asset`, `people`, `assignment`, `inventory`, `discovery`,
  `integration`, `audit`, `security`.
- El modelo se define con EF Core en `CeibaAssets.Infrastructure`. Los despliegues usan scripts
  idempotentes generados desde las migraciones (`dotnet ef migrations script --idempotent`).
- Concurrencia optimista con columnas `rowversion` en las entidades editables.
- Fechas en UTC (`datetime2`).
- Índices definidos a partir de consultas reales, no por anticipación.
- Procedimientos almacenados solo cuando estén justificados, con un ADR.
- No existe una restricción global de número de serie único. Los identificadores se normalizan
  conservando el valor original.
- Nunca se cargan datos reales.
