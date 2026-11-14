import { provideHttpClient } from '@angular/common/http';
import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import { API_BASE_URL } from '../http/generated/ceiba-api.client';

/**
 * La SPA y la API comparten origen en IIS; la API vive bajo esta ruta (ver ADR-0006).
 */
export const API_BASE_PATH = '/api';

/**
 * Registra HttpClient y la ruta base que usan los clientes generados por NSwag.
 */
export function provideCeibaApi(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(),
    { provide: API_BASE_URL, useValue: API_BASE_PATH },
  ]);
}
