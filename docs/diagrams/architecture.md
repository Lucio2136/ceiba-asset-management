# Diagramas de arquitectura

Los diagramas usan Mermaid y GitHub los muestra directamente. Los elementos de fases futuras
representan el diseño objetivo.

## Contexto

```mermaid
flowchart LR
    admin["Administración de activos"]
    soporte["Soporte técnico"]
    auditor["Auditoría"]
    equipos["Equipos Windows de CEIBA<br/>(agente PowerShell)"]
    invgate["InvGate<br/>(API oficial, solo lectura)"]

    sistema["CEIBA Asset Management"]

    admin -->|HTTPS| sistema
    soporte -->|HTTPS| sistema
    auditor -->|HTTPS| sistema
    equipos -->|"Observaciones por HTTPS"| sistema
    sistema -->|"Lectura de activos"| invgate
```

## Contenedores

```mermaid
flowchart TB
    navegador["Navegador"]

    subgraph servidor["Windows Server 2025"]
        subgraph iis["IIS: sitio HTTPS"]
            spa["SPA Angular<br/>(archivos estáticos en /)"]
            api["ASP.NET Core API<br/>(aplicación /api)"]
        end
        worker["CeibaAssets.SyncWorker<br/>(Windows Service)"]
    end

    sql[("SQL Server 2025<br/>esquemas por módulo")]

    subgraph endpoint["Equipo Windows"]
        tarea["Task Scheduler"] --> agente["CeibaAssets.Agent<br/>(PowerShell 7)"]
    end

    invgate["API de InvGate"]

    navegador -->|HTTPS| spa
    navegador -->|"HTTPS /api"| api
    agente -->|"HTTPS /api"| api
    api --> sql
    worker --> sql
    worker -->|"HTTPS, solo lectura"| invgate
```

## Flujo de descubrimiento (Fase 7 y Fase 8)

```mermaid
sequenceDiagram
    participant A as Agente PowerShell
    participant API as API
    participant D as Staging de descubrimiento
    participant C as Conciliación
    participant U as Responsable de inventario
    participant I as Inventario validado

    A->>API: POST envío (HTTPS, clave de idempotencia)
    API->>D: Guarda envío y observaciones tal como llegaron
    API->>C: Calcula posibles coincidencias
    C-->>U: Presenta coincidencias y diferencias
    U->>C: Decide (vincular, crear activo, descartar)
    C->>I: Aplica la decisión aprobada y registra auditoría
```

## Flujo de sincronización con InvGate (Fase 9)

```mermaid
sequenceDiagram
    participant W as SyncWorker
    participant G as API de InvGate
    participant S as Snapshots externos
    participant R as Conciliación
    participant U as Responsable de inventario

    W->>G: Lectura paginada (timeout, reintentos, rate limit)
    G-->>W: Activos de ubicaciones CEIBA
    W->>S: Guarda snapshot inmutable de la ejecución
    W->>R: Normaliza y detecta diferencias y posibles duplicados
    R-->>U: Diferencias pendientes
    U->>R: Aprueba o rechaza cada cambio
    Note over W,G: Nunca se escribe en las bases internas de InvGate
```
