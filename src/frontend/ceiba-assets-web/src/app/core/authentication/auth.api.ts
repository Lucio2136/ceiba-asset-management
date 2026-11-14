import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_PATH } from '../configuration/api.providers';
import { CurrentUser, LoginRequest } from '../../shared/models/auth.models';

/**
 * Facade HTTP de autenticación (`/api/v1/auth/*`).
 *
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere
 * (docs/architecture/frontend-api-facades.md). La sesión viaja en una cookie HttpOnly.
 */
@Injectable({ providedIn: 'root' })
export class AuthApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/auth`;

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${this.baseUrl}/login`, request);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {});
  }

  me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`);
  }
}
