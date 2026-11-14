# ADR-0010: Observabilidad y manejo de errores

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

La operación en Windows Server requiere logs persistentes, poder correlacionar un error visto
por un usuario con los registros del servidor, y respuestas de error que no expongan detalles
internos.

## Decisión

### Logs

- **Serilog** registrado con `AddSerilog` y configuración en `appsettings`. Cada host crea su
  propio logger, de modo que las pruebas con varios hosts en paralelo no comparten estado.
- `appsettings.json` define niveles y enriquecimiento pero **ningún destino**. Development agrega
  consola y archivo JSON compacto (`logs/`, rotación diaria y retención de 7 días). La ruta
  absoluta de producción se definirá en la Fase 10.
- `UseSerilogRequestLogging` registra un evento por solicitud. No se registran cuerpos,
  cookies, contraseñas ni tokens.

### Correlación

- `X-Correlation-ID`: se acepta el valor del cliente solo si cumple `^[A-Za-z0-9._:-]{1,64}$`; si
  no, se genera un GUID.
- El identificador se devuelve en la respuesta, se agrega al ámbito de logging (`CorrelationId`)
  y se incluye en Problem Details.
- El middleware escribe sus cabeceras en `OnStarting`, para conservarlas cuando el manejador de
  excepciones reinicia la respuesta.

### Errores

- `AddProblemDetails` con `UseExceptionHandler` y `UseStatusCodePages`: toda respuesta de error
  es `application/problem+json` con `traceId` y `correlationId`.
- Fuera de Development no se exponen trazas de pila ni mensajes internos.
- El rate limiting responde 429 con Problem Details y `Retry-After`.

### Salud

- `/health/live`: verifica solo que el proceso responde, sin dependencias.
- `/health/ready`: ejecuta los checks etiquetados `ready` (la base de datos se agregará en la
  Fase 2).
- Las respuestas incluyen estado y duración, sin descripciones ni excepciones. Ambos endpoints
  están exentos de rate limiting.

## Alternativas consideradas

- **Solo `ILogger` con proveedores integrados**: el proveedor de archivo no es parte de la
  plataforma, y el registro de eventos de Windows no se presta a búsquedas por correlación.
- **Logger estático de arranque de Serilog**: causa conflictos cuando varias pruebas crean hosts
  en paralelo.

## Consecuencias

- Un usuario puede reportar el `correlationId` de un error y operaciones lo localiza en los logs.
- Los fallos durante el arranque en IIS se diagnostican con el registro de eventos de Windows y
  los logs de stdout del ASP.NET Core Module (Fase 10).
