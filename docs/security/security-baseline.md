# Línea base de seguridad

## Principios

- Solo datos ficticios. No se usan datos, credenciales, nombres, direcciones ni infraestructura
  reales de ninguna organización.
- Mínimo privilegio en cuentas de usuario, de servicio y de SQL Server.
- La validación y la autorización ocurren en el backend; la validación del frontend solo mejora la
  experiencia de usuario.
- Configuración segura por defecto: lo que no está habilitado de forma explícita queda cerrado.
- Los secretos nunca se versionan.

## Clasificación de datos

| Datos                                    | Clasificación  | Tratamiento                                    |
| ---------------------------------------- | -------------- | ---------------------------------------------- |
| Inventario, ubicaciones, responsables    | Confidencial interno | Acceso autenticado y autorizado por rol   |
| Eventos de auditoría                     | Confidencial interno | Solo inserción; acceso restringido        |
| Credenciales y secretos                  | Secreto        | Fuera de Git; almacenamiento protegido         |

## Controles implementados en la Fase 1

| Control                                   | Implementación                                                         | Verificación                         |
| ----------------------------------------- | ---------------------------------------------------------------------- | ------------------------------------ |
| HTTPS                                     | `UseHttpsRedirection`; HSTS fuera de Development                        | Configuración del pipeline           |
| Cabeceras de seguridad                    | `nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, `Cross-Origin-Resource-Policy: same-origin`, CSP `default-src 'none'; frame-ancestors 'none'` | `SecurityHeadersTests` |
| Sin caché de respuestas sensibles         | `Cache-Control: no-store` por defecto en la API                         | `SecurityHeadersTests`               |
| Sin cabecera `Server`                     | `AddServerHeader = false` en Kestrel (IIS se configura en la Fase 10)   | Prueba manual con `curl`             |
| Rate limiting                             | Límite global por IP en ventana fija; 429 con Problem Details y `Retry-After` | `RateLimitingTests`           |
| Configuración validada al arranque        | `ValidateDataAnnotations` y `ValidateOnStart`                           | `RateLimitingTests`                  |
| Errores sin detalles internos             | Problem Details sin trazas de pila fuera de Development                 | `ErrorHandlingTests`                 |
| Correlación segura                        | `X-Correlation-ID` con formato restringido; valores inseguros se reemplazan | `CorrelationIdTests`             |
| Swagger restringido                       | Solo con `OpenApi:Enabled=true` (Development)                           | `ErrorHandlingTests`                 |
| Hosts permitidos                          | `AllowedHosts=localhost` por defecto; producción define su FQDN         | Configuración                        |
| Secretos fuera de Git                     | `.gitignore` para `.env`, `*.pfx`, `*.key`, `*.pem`, `appsettings.*.local.json`, respaldos | Revisión de `git status` |
| Auditoría de dependencias                 | NuGet Audit (directas y transitivas) como error; `npm audit` en 0 vulnerabilidades | Compilación y `npm audit` |
| Sin recursos de CDN externos              | Fuentes del sistema; los íconos se empaquetan con la aplicación         | `index.html`                         |
| Logs sin datos sensibles                  | Un evento por solicitud, sin cuerpos, cookies ni cabeceras de autenticación | Configuración de Serilog          |

La compresión de respuestas está habilitada sobre HTTPS. Para evitar ataques tipo BREACH, ninguna
respuesta comprimida debe combinar secretos con datos controlados por el cliente; los tokens
antiforgery viajarán en cookies y cabeceras, no en el cuerpo.

## Controles planificados

| Control                                                       | Fase |
| ------------------------------------------------------------- | ---- |
| Cuentas SQL separadas (migraciones y ejecución) con mínimo privilegio | 2 y 10 |
| ASP.NET Core Identity, cookie HttpOnly, antiforgery, bloqueo por intentos, auditoría de accesos | 3 |
| Autorización por roles y políticas, política de autorización por defecto | 3 |
| Concurrencia optimista con `rowversion`                       | 2 y 4 |
| Modelo de amenazas y autenticación del agente                 | 7    |
| Idempotencia y límites de tamaño en la ingestión del agente   | 7    |
| Almacenamiento protegido de credenciales de InvGate           | 9    |
| Certificados TLS, bindings, firewall, permisos NTFS, cuentas de servicio | 10 |
| Cabeceras de seguridad y CSP de la SPA en IIS                 | 10   |
| Análisis estático en CI (SonarQube) y auditoría de dependencias | 11   |
| Respaldos protegidos, pruebas de restauración, hardening      | 12   |

## Riesgos aceptados y seguimiento

| Tema                                         | Estado                                                                 |
| -------------------------------------------- | ---------------------------------------------------------------------- |
| Override de `minimatch` en herramientas de lint | Temporal; se retira cuando typescript-eslint actualice su dependencia ([ADR-0009](../adr/0009-versiones-y-dependencias.md)) |
| Certificado de desarrollo de ASP.NET Core    | Solo para desarrollo local; el proxy de Angular no valida el certificado (`secure: false`) |
