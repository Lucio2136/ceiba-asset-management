# ADR-0001: Registrar las decisiones de arquitectura

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

CEIBA Asset Management se construye en doce fases. Muchas decisiones tempranas (topología de
hospedaje, contrato de la API, autenticación, modelo de datos) condicionan las fases posteriores.
Sin un registro, el motivo de cada decisión se pierde y se reabren discusiones ya resueltas.

## Decisión

Registraremos cada decisión significativa como un ADR en `docs/adr/`, numerado de forma
secuencial y con la estructura de `0000-plantilla.md`. Un ADR aceptado es inmutable en su
decisión; si cambia, se crea uno nuevo que lo reemplaza. Las decisiones que dependen de
información aún no disponible se listan como pendientes en el índice.

## Alternativas consideradas

- Documentar decisiones en el README o en descripciones de pull requests: se dispersan y no
  conservan el contexto.
- Una wiki externa: separa la decisión del código que la implementa.

## Consecuencias

- Cada fase revisa el índice y agrega los ADR que correspondan.
- Los pull requests que contradigan un ADR aceptado deben incluir el ADR que lo reemplaza.
