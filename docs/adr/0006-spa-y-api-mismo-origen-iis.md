# ADR-0006: SPA y API en el mismo origen dentro de IIS

- Estado: Aceptada
- Fecha: 2026-11-13

## Contexto

La autenticación de laboratorio usará cookies HttpOnly (ADR-0007). Con orígenes distintos para
la SPA y la API se necesitarían CORS con credenciales, cookies con `SameSite=None` y una
configuración de CSP más amplia.

## Decisión

- Un sitio de IIS con enlace HTTPS (dominio público por definir, ver ADR-0011) sirve la SPA como
  archivos estáticos en la raíz.
- La API se publica como **aplicación hija en `/api`** del mismo sitio, con su propio grupo de
  aplicaciones ("No Managed Code") y el ASP.NET Core Module en modo in-process.
- La regla de reescritura de la SPA (fallback a `index.html`) excluye `/api`, y la aplicación
  hija no hereda las reglas de reescritura del sitio.
- En desarrollo, el servidor de Angular redirige `/api` a `https://localhost:7135` mediante
  `proxy.conf.json`, y la API usa `Hosting:PathBase=/api` para reproducir la ruta de IIS.
- El cliente Angular usa la ruta relativa `/api` como `API_BASE_URL`.

## Alternativas consideradas

- **Nombres de host separados**: requieren CORS con credenciales y cookies menos restrictivas.
- **La API sirve los archivos de la SPA**: acopla los despliegues de frontend y backend y pierde
  la caché de archivos estáticos de IIS.
- **Proxy inverso con ARR**: agrega un módulo de IIS sin beneficio frente a la aplicación hija.

## Consecuencias

- No se habilita CORS. Si en el futuro se requiere, se usará una lista explícita de orígenes.
- Las cookies pueden usar `SameSite=Strict`.
- El `web.config` del sitio y el de la API deben coordinarse (Fase 10).
- Fuera de Development, `Hosting:PathBase` queda vacío porque IIS aporta la ruta base.
