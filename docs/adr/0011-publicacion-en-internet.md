# ADR-0011: Publicación de la aplicación en internet

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

El responsable del proyecto confirmó que CEIBA Asset Management se publicará en la web pública y
que no está destinado a una red corporativa interna. El proyecto conserva el stack acordado
(Windows Server 2025, IIS, SQL Server 2025) y utiliza exclusivamente datos ficticios.

## Decisión

- El entorno objetivo es un servidor Windows Server 2025 accesible desde internet, con la misma
  topología de mismo origen en IIS (ADR-0006).
- Exposición mínima a internet: solo HTTPS (443) y HTTP (80) para redirigir a HTTPS y validar el
  certificado. SQL Server, el escritorio remoto y cualquier puerto de administración no se
  exponen públicamente.
- Certificado TLS emitido por una autoridad pública, con renovación automática.
- **SQL Server 2025 Express** en desarrollo y en el servidor: está licenciado para producción sin
  costo, a diferencia de la edición Developer. Sus límites de recursos se revisarán al instalarlo
  en la Fase 2 frente al volumen previsto.
- Los controles pensados para una red interna se tratan como requisitos de un sistema expuesto a
  internet (ver la [línea base de seguridad](../security/security-baseline.md)).

## Alternativas consideradas

- **Red interna**: descartada por el responsable del proyecto.
- **SQL Server Developer en el servidor**: no está licenciado para producción.
- **Servicios administrados en la nube** (por ejemplo, base de datos administrada): requieren
  elegir proveedor y presupuesto; se evaluarán junto con el hospedaje en la Fase 10.

## Consecuencias

- La elección del proveedor de hospedaje y del dominio público queda pendiente.
- SQL Server Express no incluye SQL Server Agent: respaldos y mantenimiento se programarán con
  Task Scheduler (Fase 12).
- El agente PowerShell enviará datos a través de internet, lo que eleva la exigencia del modelo
  de amenazas y de su autenticación (Fase 7).
- Se requieren monitoreo y alertas del endpoint público (Fase 12).
