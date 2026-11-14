# Registro de decisiones de arquitectura (ADR)

Cada decisión con impacto en la estructura, la seguridad, la operación o el costo de cambio se
registra aquí con la [plantilla](0000-plantilla.md). Un ADR aceptado no se edita para cambiar su
decisión: se reemplaza con uno nuevo que lo referencia.

| ADR                                                   | Título                                                       | Estado                         |
| ----------------------------------------------------- | ------------------------------------------------------------ | ------------------------------ |
| [0001](0001-registro-de-decisiones.md)                | Registrar las decisiones de arquitectura                     | Aceptada                       |
| [0002](0002-monolito-modular-windows-iis.md)          | Monolito modular sobre Windows Server e IIS, sin contenedores | Aceptada                       |
| [0003](0003-organizacion-del-repositorio.md)          | Organización del repositorio y límites entre proyectos       | Aceptada                       |
| [0004](0004-contrato-openapi-nswag.md)                | OpenAPI como contrato y NSwag como generador                 | Aceptada                       |
| [0005](0005-controladores-y-rutas-versionadas.md)     | Controladores ASP.NET Core y rutas versionadas               | Aceptada                       |
| [0006](0006-spa-y-api-mismo-origen-iis.md)            | SPA y API en el mismo origen dentro de IIS                   | Aceptada                       |
| [0007](0007-autenticacion-laboratorio-identity.md)    | Autenticación de laboratorio con ASP.NET Core Identity       | Aceptada (se implementa en Fase 3) |
| [0008](0008-calidad-y-pruebas.md)                     | Herramientas de calidad y estrategia de pruebas              | Aceptada                       |
| [0009](0009-versiones-y-dependencias.md)              | Política de versiones y dependencias                         | Aceptada                       |
| [0010](0010-observabilidad-y-errores.md)              | Observabilidad y manejo de errores                           | Aceptada                       |

## Decisiones pendientes

Estas decisiones se tomarán cuando exista la información necesaria; no deben asumirse antes.

| Tema                                                                 | Fase | Condición para decidir                         |
| -------------------------------------------------------------------- | ---- | ---------------------------------------------- |
| Autenticación del agente: certificado de cliente o credenciales de máquina | 7    | Modelo de amenazas del agente documentado      |
| Control de concurrencia expuesto en la API (rowversion como ETag o campo) | 2    | Primer recurso editable                         |
| Migración a Microsoft Entra ID con OpenID Connect                    | —    | Confirmación de un tenant empresarial          |
| Cobertura de código y análisis con SonarQube                         | 11   | Servidor SonarQube y token disponibles         |
| Internacionalización                                                 | —    | Requisito multidioma confirmado                |
