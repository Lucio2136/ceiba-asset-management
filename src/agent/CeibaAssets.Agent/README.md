# CeibaAssets.Agent

Agente de descubrimiento para equipos Windows. Se implementa en la Fase 7.

## Alcance previsto

- PowerShell 7, ejecutado manualmente o por Task Scheduler.
- Fabricante, modelo, serial del BIOS y UUID mediante CIM y WMI.
- Monitores mediante EDID cuando sea posible; webcams y otros dispositivos PnP.
- Identificador de instalación, fecha UTC y versión del agente en cada envío.
- Validación y normalización conservando los valores originales.
- Envío por HTTPS con clave de idempotencia, reintentos seguros y logs locales.
- Pruebas Pester con fixtures de respuestas CIM y PnP.

## Restricciones

- No se asume que monitores, webcams o periféricos expongan un número de serie.
- Las observaciones llegan a un área de staging; nunca se convierten automáticamente en activos.
- No se incrustan secretos en los scripts.
- La autenticación (certificado de cliente o credenciales de máquina) se decide con un ADR tras un
  modelo de amenazas.
