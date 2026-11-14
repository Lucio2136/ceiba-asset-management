# Runbooks

Procedimientos operativos. Los runbooks completos (despliegue, rollback, respaldo y
restauración, renovación de certificados, diagnóstico del agente y de la sincronización) se
entregan en las Fases 10 y 12.

## Diagnóstico de un error con el Correlation ID

Disponible desde la Fase 1.

1. Obtén el `correlationId` del error: viene en la respuesta Problem Details y en la cabecera
   `X-Correlation-ID`.
2. Localiza los eventos en los logs JSON de la API (en desarrollo,
   `src/backend/CeibaAssets.Api/logs/`):

   ```powershell
   Select-String -Path .\logs\ceiba-assets-api-*.log -Pattern '"CorrelationId":"<valor>"'
   ```

3. El evento de la solicitud incluye ruta, código de estado y duración. Los eventos de error
   incluyen la excepción, que nunca se expone al cliente.
