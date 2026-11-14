# ADR-0002: Monolito modular sobre Windows Server e IIS, sin contenedores

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

- El dominio cubre activos tecnológicos de tres edificios ficticios (19 niveles en total). El
  volumen esperado es de miles de activos, no de millones.
- El entorno objetivo es un ecosistema Microsoft: Windows 11 para desarrollo, Windows Server 2025
  con IIS para despliegue y SQL Server 2025.
- Restricción explícita: no usar Docker, Docker Compose, Kubernetes ni contenedores, ni
  microservicios sin una necesidad demostrable.
- Los procesos de negocio (asignación, inventario físico, conciliación, auditoría) cruzan varios
  módulos y requieren consistencia transaccional.

## Decisión

Construiremos un monolito modular compuesto por:

1. **SPA Angular** publicada como archivos estáticos en IIS.
2. **ASP.NET Core Web API** (.NET 10 LTS) hospedada en IIS con el ASP.NET Core Module, que
   contiene todos los módulos funcionales.
3. **SQL Server 2025** con un esquema por módulo (`location`, `asset`, `people`, `assignment`,
   `inventory`, `discovery`, `integration`, `audit`, `security`).
4. **CeibaAssets.SyncWorker**, un Worker .NET instalado como Windows Service para la
   sincronización de solo lectura con InvGate.
5. **Agente PowerShell 7** ejecutado por Task Scheduler en los equipos, que envía
   observaciones a la API por HTTPS.

Los módulos se separan por carpetas y espacios de nombres dentro de cada capa, y por esquemas en
la base de datos. Las reglas de dependencia entre capas se verifican con pruebas de arquitectura.

## Alternativas consideradas

- **Microservicios**: multiplican despliegues, observabilidad y consistencia distribuida sin un
  requisito de escala o de equipos independientes que lo justifique.
- **Contenedores**: excluidos por restricción del proyecto.
- **Un único proceso que incluya la sincronización con InvGate**: acoplaría la disponibilidad de
  la API a una integración externa con reintentos y ventanas largas. El Worker separado comparte
  las capas Application e Infrastructure sin compartir el proceso.

## Consecuencias

- Un solo artefacto de API por despliegue y transacciones locales entre módulos.
- La escalabilidad es vertical (recursos del servidor y del grupo de aplicaciones de IIS); es
  suficiente para el volumen esperado.
- Los límites entre módulos dependen de disciplina: se refuerzan con pruebas de arquitectura y
  con esquemas SQL separados.
- El Worker y la API comparten código de aplicación, por lo que ambos se versionan y despliegan
  juntos.
