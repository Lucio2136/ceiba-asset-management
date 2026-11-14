import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, Observable, of } from 'rxjs';
import { AuthService } from './auth.service';

function resolveAuthenticated(auth: AuthService): Observable<boolean> {
  if (auth.status() !== 'unknown') {
    return of(auth.isAuthenticated());
  }
  // Un fallo distinto de 401 (red, 5xx) se trata como «sin sesión»; el login mostrará el error.
  return auth.loadCurrentUser().pipe(
    map((user) => user !== null),
    catchError(() => {
      auth.clear();
      return of(false);
    }),
  );
}

/** Exige sesión; si no hay, redirige a `/login` conservando la URL solicitada. */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return resolveAuthenticated(auth).pipe(
    map((authenticated) =>
      authenticated
        ? true
        : router.createUrlTree(['/login'], {
            queryParams: state.url && state.url !== '/' ? { returnUrl: state.url } : {},
          }),
    ),
  );
};

/** Solo para visitantes: si ya hay sesión, redirige a `/assets`. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return resolveAuthenticated(auth).pipe(
    map((authenticated) => (authenticated ? router.createUrlTree(['/assets']) : true)),
  );
};
