import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CurrentUser } from '../../shared/models/auth.models';
import { provideCeibaApi } from '../configuration/api.providers';
import { AuthService } from './auth.service';

const user: CurrentUser = {
  id: '1',
  email: 'admin@ceiba.example',
  displayName: 'Administrador',
  roles: ['Admin'],
};

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideCeibaApi(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    document.cookie = 'XSRF-TOKEN=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
  });

  it('empieza con la sesión desconocida', () => {
    expect(service.status()).toBe('unknown');
    expect(service.isAuthenticated()).toBe(false);
    expect(service.user()).toBeNull();
  });

  it('login publica el usuario, envía cookies y marca la sesión como autenticada', async () => {
    const result = firstValueFrom(
      service.login({ email: 'admin@ceiba.example', password: 'secreto' }),
    );

    const request = http.expectOne('/api/v1/auth/login');
    expect(request.request.method).toBe('POST');
    expect(request.request.withCredentials).toBe(true);
    expect(request.request.body).toEqual({ email: 'admin@ceiba.example', password: 'secreto' });
    request.flush(user);

    await expect(result).resolves.toEqual(user);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.user()).toEqual(user);
  });

  it('copia la cookie XSRF-TOKEN a la cabecera X-XSRF-TOKEN en peticiones que modifican estado', async () => {
    document.cookie = 'XSRF-TOKEN=token-123; path=/';

    const result = firstValueFrom(service.logout());
    const request = http.expectOne('/api/v1/auth/logout');
    expect(request.request.headers.get('X-XSRF-TOKEN')).toBe('token-123');
    request.flush(null);

    await result;
  });

  it('logout descarta el usuario', async () => {
    const login = firstValueFrom(service.login({ email: 'admin@ceiba.example', password: 'x' }));
    http.expectOne('/api/v1/auth/login').flush(user);
    await login;

    const logout = firstValueFrom(service.logout());
    http.expectOne('/api/v1/auth/logout').flush(null);
    await logout;

    expect(service.user()).toBeNull();
    expect(service.status()).toBe('anonymous');
  });

  it('logout limpia la sesión local aunque el servidor falle', async () => {
    const login = firstValueFrom(service.login({ email: 'admin@ceiba.example', password: 'x' }));
    http.expectOne('/api/v1/auth/login').flush(user);
    await login;

    const logout = firstValueFrom(service.logout());
    http.expectOne('/api/v1/auth/logout').flush(null, { status: 500, statusText: 'Error' });

    await expect(logout).rejects.toBeTruthy();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('loadCurrentUser carga el usuario cuando hay sesión', async () => {
    const result = firstValueFrom(service.loadCurrentUser());
    http.expectOne('/api/v1/auth/me').flush(user);

    await expect(result).resolves.toEqual(user);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('loadCurrentUser interpreta un 401 como «sin sesión»', async () => {
    const result = firstValueFrom(service.loadCurrentUser());
    http.expectOne('/api/v1/auth/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    await expect(result).resolves.toBeNull();
    expect(service.status()).toBe('anonymous');
  });

  it('loadCurrentUser propaga los errores que no son 401', async () => {
    const result = firstValueFrom(service.loadCurrentUser());
    http.expectOne('/api/v1/auth/me').flush(null, { status: 503, statusText: 'Unavailable' });

    await expect(result).rejects.toMatchObject({ status: 503 });
    expect(service.status()).toBe('unknown');
  });
});
