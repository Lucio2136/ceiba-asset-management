import { provideHttpClient, withInterceptors, withXsrfConfiguration } from '@angular/common/http';
import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { API_BASE_URL } from '../http/generated/ceiba-api.client';
import { credentialsInterceptor } from '../http/credentials.interceptor';
import { unauthorizedInterceptor } from '../http/unauthorized.interceptor';

/**
 * La SPA y la API comparten origen en IIS; la API vive bajo esta ruta (ver ADR-0006).
 */
export const API_BASE_PATH = '/api';

/** Nombres antiforgery acordados con el backend (ADR-0007). */
export const XSRF_COOKIE_NAME = 'XSRF-TOKEN';
export const XSRF_HEADER_NAME = 'X-XSRF-TOKEN';

/**
 * Registra HttpClient (cookies de sesión + antiforgery + manejo de 401) y la ruta base que
 * usan los clientes generados por NSwag.
 */
export function provideCeibaApi(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(
      withInterceptors([credentialsInterceptor, unauthorizedInterceptor]),
      withXsrfConfiguration({ cookieName: XSRF_COOKIE_NAME, headerName: XSRF_HEADER_NAME }),
    ),
    { provide: API_BASE_URL, useValue: API_BASE_PATH },
  ]);
}
