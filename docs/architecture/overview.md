# Arquitectura de CEIBA Asset Management

## Propósito

Administrar, descubrir, conciliar, asignar y auditar los activos tecnológicos de CEIBA Torre A
(8 niveles), CEIBA Torre B (10 niveles) y CEIBA Ecommerce (1 nivel). Todos los datos son ficticios.

## Atributos de calidad prioritarios

| Atributo         | Cómo se atiende                                                                              |
| ---------------- | -------------------------------------------------------------------------------------------- |
| Seguridad        | HTTPS, mínimo privilegio, validación en backend, cookies HttpOnly, auditoría ([línea base](../security/security-baseline.md)) |
| Auditabilidad    | Historial de cambios, decisiones de conciliación registradas, correlación de solicitudes     |
| Integridad       | Separación entre observaciones y activos validados; concurrencia optimista con `rowversion`  |
| Mantenibilidad   | Monolito modular con reglas de dependencia verificadas por pruebas                            |
| Operabilidad     | Health checks, logs estructurados, despliegue y runbooks documentados                         |
| Rendimiento      | Paginación, filtros y agregaciones en el servidor; carga diferida en la SPA                   |

## Componentes

Ver los [diagramas](../diagrams/architecture.md).

| Componente                 | Tecnología                         | Responsabilidad                                                      |
| -------------------------- | ---------------------------------- | -------------------------------------------------------------------- |
| SPA                        | Angular 22, Angular Material       | Interfaz de usuario; nunca accede a SQL Server                       |
| API                        | ASP.NET Core (.NET 10), EF Core    | Casos de uso, autorización, validación, auditoría                    |
| Base de datos              | SQL Server 2025                    | Persistencia con un esquema por módulo                               |
| CeibaAssets.SyncWorker     | Worker .NET como Windows Service   | Sincronización de solo lectura con la API oficial de InvGate         |
| CeibaAssets.Agent          | PowerShell 7 con Task Scheduler    | Descubrimiento con CIM, WMI y PnP; envío por HTTPS a la API          |
| Hospedaje                  | Windows Server 2025, IIS           | SPA y API en el mismo origen ([ADR-0006](../adr/0006-spa-y-api-mismo-origen-iis.md)) |

## Módulos funcionales

| Módulo       | Esquema SQL   | Contenido principal                                                          | Fase |
| ------------ | ------------- | ---------------------------------------------------------------------------- | ---- |
| Ubicaciones  | `location`    | Edificios, pisos, ubicaciones                                                | 2    |
| Seguridad    | `security`    | Identity, roles, accesos                                                     | 3    |
| Auditoría    | `audit`       | Eventos de auditoría                                                         | 3    |
| Activos      | `asset`       | Activos, tipos, estados, identificadores, perfiles por tipo, relaciones      | 4    |
| Personas     | `people`      | Responsables                                                                 | 5    |
| Asignaciones | `assignment`  | Asignaciones, devoluciones, movimientos                                      | 5    |
| Descubrimiento | `discovery` | Instalaciones del agente, envíos, observaciones de dispositivos y componentes | 7    |
| Inventario   | `inventory`   | Sesiones de inventario físico, observaciones, discrepancias                  | 8    |
| Integración  | `integration` | Sistemas externos, ejecuciones de sincronización, snapshots, diferencias, decisiones | 9    |

Los mantenimientos y los reportes se apoyan en estos módulos; su ubicación se definirá al
implementarlos.

## Separación de datos y conciliación

Se mantienen separados, en tablas y responsabilidades distintas:

1. **Activos validados**: el inventario oficial.
2. **Observaciones del agente**: lo que reporta cada equipo, tal como llegó.
3. **Snapshots de InvGate**: copias inmutables de lo leído en cada sincronización.
4. **Posibles coincidencias**: candidatos a corresponder con un activo validado.
5. **Diferencias pendientes**: discrepancias detectadas entre fuentes.
6. **Decisiones de conciliación**: quién decidió qué, cuándo y por qué.

Reglas:

- Ningún dispositivo detectado se convierte automáticamente en activo oficial.
- No existe una restricción global de número de serie único: hay fabricantes con seriales vacíos,
  genéricos o duplicados. Los identificadores se normalizan conservando el valor original.
- Los cambios provenientes de fuentes externas requieren una decisión aprobada antes de
  aplicarse.
- Nunca se escribe en las bases internas de InvGate.

## Backend

Capas y reglas de dependencia en [ADR-0003](../adr/0003-organizacion-del-repositorio.md).
Convenciones en [conventions.md](conventions.md).

Canal de una solicitud en la API (orden del pipeline):

1. `UsePathBase` (solo si `Hosting:PathBase` tiene valor).
2. Correlation ID y cabeceras de seguridad.
3. Registro de solicitudes (Serilog).
4. Manejador de excepciones y páginas de estado en Problem Details.
5. HSTS (fuera de Development), redirección HTTPS y compresión de respuestas.
6. Swagger UI (solo si `OpenApi:Enabled`).
7. Enrutamiento, rate limiting y autorización.
8. Controladores y health checks.

## Frontend

- `core/`: configuración, estructura de la aplicación (shell, inicio, 404), servicios únicos y
  el cliente HTTP generado.
- `shared/`: componentes, directivas y utilidades con una responsabilidad reutilizable clara.
- `features/`: una carpeta por vertical funcional, con rutas lazy-loaded.

Criterios de interfaz:

| Necesidad              | Solución                                                                              |
| ---------------------- | ------------------------------------------------------------------------------------- |
| Formularios            | Angular Material con Signal Forms (API pública desde Angular 22); Reactive Forms solo para formularios muy dinámicos |
| Listados sencillos     | Material Table con MatPaginator y MatSort                                              |
| Inventarios complejos  | AG Grid Community con paginación, filtros y orden en el servidor (sin funciones Enterprise como Server-Side Row Model) |
| Dashboards             | MatCard y ECharts (ngx-echarts) cargados con `@defer`; agregaciones en el backend       |
| Selectores             | MatSelect, MatAutocomplete con búsqueda en servidor y debounce, CDK Virtual Scroll     |
| Notificaciones         | MatSnackBar; confirmaciones con MatDialog                                              |
| Íconos                 | Lucide (`@lucide/angular`)                                                             |
| Fechas                 | Material Datepicker; DatePipe e Intl para mostrar; date-fns para cálculos; UTC en API  |

Toda pantalla distingue los estados de carga, contenido, vacío, error y sin permisos.

Estado: signals para estado local y derivado, RxJS para HTTP y flujos asíncronos, sin NgRx. Los
componentes no usan `HttpClient`; consumen servicios o facades que usan el cliente generado.

## Hoja de ruta

| Fase | Alcance                                                                 | Estado     |
| ---- | ----------------------------------------------------------------------- | ---------- |
| 1    | Repositorio, herramientas, estructura, calidad, documentación           | Completada |
| 2    | SQL Server, EF Core, edificios, pisos, ubicaciones, seed ficticio       | Pendiente  |
| 3    | Identity, inicio de sesión, autorización, auditoría inicial             | Pendiente  |
| 4    | Activos e identificadores, inventario con AG Grid, formularios, detalle | Pendiente  |
| 5    | Responsables, asignaciones, devoluciones, movimientos                   | Pendiente  |
| 6    | Dashboard con ECharts                                                   | Pendiente  |
| 7    | Agente PowerShell e ingestión                                           | Pendiente  |
| 8    | Inventarios físicos y conciliación                                      | Pendiente  |
| 9    | Integración ficticia con InvGate y conexión oficial preparada           | Pendiente  |
| 10   | IIS, Windows Service, Task Scheduler, TLS, firewall                     | Pendiente  |
| 11   | CI/CD con GitHub Actions                                                | Pendiente  |
| 12   | Backups, restauración, monitoreo, hardening, runbooks                   | Pendiente  |
