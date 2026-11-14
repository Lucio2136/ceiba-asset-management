/*
 * DTOs TEMPORALES del contrato `/api/v1/auth/*`.
 * Se reemplazan por el cliente NSwag cuando el backend lo regenere.
 */

/** Credenciales del formulario de inicio de sesión. */
export interface LoginRequest {
  email: string;
  password: string;
}

/** Usuario autenticado (`GET /api/v1/auth/me` y `POST /api/v1/auth/login`). */
export interface CurrentUser {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
}
