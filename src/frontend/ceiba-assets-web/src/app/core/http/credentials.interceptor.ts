import { HttpInterceptorFn } from '@angular/common/http';
import { API_BASE_PATH } from '../configuration/api.providers';

/**
 * Envía las cookies de sesión en las peticiones a la API (`withCredentials`).
 * El token antiforgery lo añade `withXsrfConfiguration` (cookie XSRF-TOKEN → X-XSRF-TOKEN).
 */
export const credentialsInterceptor: HttpInterceptorFn = (request, next) =>
  request.url.startsWith(`${API_BASE_PATH}/`)
    ? next(request.clone({ withCredentials: true }))
    : next(request);
