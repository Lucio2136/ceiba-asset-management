import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../authentication/auth.service';
import { API_BASE_PATH } from '../configuration/api.providers';

const AUTH_PATH = `${API_BASE_PATH}/v1/auth/`;

/**
 * Si la API responde 401 a una petición normal, la sesión venció: se descarta el estado local
 * y se lleva al usuario a `/login`. Los endpoints `/auth/*` gestionan su propio 401.
 */
export const unauthorizedInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        request.url.startsWith(`${API_BASE_PATH}/`) &&
        !request.url.startsWith(AUTH_PATH) &&
        auth.isAuthenticated()
      ) {
        auth.clear();
        void router.navigate(['/login'], { queryParams: { returnUrl: router.url } });
      }
      return throwError(() => error);
    }),
  );
};
