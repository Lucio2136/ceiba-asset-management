# ADR-0007: Autenticación de laboratorio con ASP.NET Core Identity

- Estado: Aceptada (se implementa en la Fase 3)
- Fecha: 2026-11-13

## Contexto

No hay un tenant empresarial confirmado de Microsoft Entra ID. El laboratorio necesita
autenticación propia, segura y auditable, sin tokens accesibles desde JavaScript.

## Decisión

- **ASP.NET Core Identity** con almacenamiento en SQL Server (esquema `security`).
- Sesión mediante **cookie HttpOnly**, `Secure` y `SameSite=Strict` (posible por ADR-0006).
- **Antiforgery** con cookie `XSRF-TOKEN` y cabecera `X-XSRF-TOKEN`, los nombres que Angular
  `HttpClient` usa por defecto. Se valida en todas las operaciones que modifican estado.
- Guards de rutas en Angular solo para experiencia de usuario. La autorización real se aplica en
  la API con políticas por rol.
- Bloqueo temporal por intentos fallidos, rate limiting específico para el inicio de sesión y
  auditoría de accesos (éxitos, fallos y bloqueos), sin registrar contraseñas.
- Sin registro público de usuarios y sin credenciales predeterminadas en el código. El primer
  administrador se crea con un procedimiento de aprovisionamiento que recibe el secreto en tiempo
  de ejecución.
- No se guardan tokens en `localStorage` ni en `sessionStorage`.

## Alternativas consideradas

- **Microsoft Entra ID (OIDC)**: es la opción preferida si se confirma un tenant empresarial; su
  adopción requerirá un ADR que reemplace a este.
- **Tokens JWT en el navegador**: exponen credenciales a JavaScript y complican la revocación.

## Consecuencias

- La autenticación del agente PowerShell es independiente de esta decisión y se resolverá en la
  Fase 7, tras un modelo de amenazas.
- Una migración futura a Entra ID cambiará el proveedor de identidad, pero conservará el modelo
  de cookie y la autorización por políticas.
