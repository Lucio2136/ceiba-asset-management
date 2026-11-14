import { HttpErrorResponse } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { catchError, map, Observable, of, tap, throwError } from 'rxjs';
import { CurrentUser, LoginRequest } from '../../shared/models/auth.models';
import { AuthApi } from './auth.api';

/** `unknown` hasta que se consulta `/auth/me` por primera vez. */
export type AuthStatus = 'unknown' | 'authenticated' | 'anonymous';

/**
 * Estado de la sesión del usuario. La credencial es una cookie HttpOnly: la SPA solo conserva
 * el perfil público en memoria (nunca tokens en localStorage/sessionStorage).
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApi);

  private readonly userState = signal<CurrentUser | null>(null);
  private readonly statusState = signal<AuthStatus>('unknown');

  readonly user = this.userState.asReadonly();
  readonly status = this.statusState.asReadonly();
  readonly isAuthenticated = computed(() => this.statusState() === 'authenticated');

  login(request: LoginRequest): Observable<CurrentUser> {
    return this.api.login(request).pipe(tap((user) => this.setUser(user)));
  }

  logout(): Observable<void> {
    return this.api.logout().pipe(
      // Aunque el servidor falle, la sesión local se descarta para no dejar la UI autenticada.
      catchError((error: unknown) => {
        this.clear();
        return throwError(() => error);
      }),
      tap(() => this.clear()),
    );
  }

  /**
   * Consulta el usuario actual. Un 401 se interpreta como «sin sesión» (no es un error);
   * cualquier otro fallo se propaga.
   */
  loadCurrentUser(): Observable<CurrentUser | null> {
    return this.api.me().pipe(
      tap((user) => this.setUser(user)),
      map((user): CurrentUser | null => user),
      catchError((error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.clear();
          return of(null);
        }
        return throwError(() => error);
      }),
    );
  }

  /** Marca la sesión como vencida (por ejemplo, tras un 401 en cualquier petición). */
  clear(): void {
    this.userState.set(null);
    this.statusState.set('anonymous');
  }

  private setUser(user: CurrentUser): void {
    this.userState.set(user);
    this.statusState.set('authenticated');
  }
}
