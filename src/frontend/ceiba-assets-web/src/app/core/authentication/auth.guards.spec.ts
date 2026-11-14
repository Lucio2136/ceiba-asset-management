import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  CanActivateFn,
  provideRouter,
  Router,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { firstValueFrom, isObservable, Observable, of } from 'rxjs';
import { AuthService, AuthStatus } from './auth.service';
import { authGuard, guestGuard } from './auth.guards';

describe('guards de autenticación', () => {
  const status = signal<AuthStatus>('unknown');
  const loadCurrentUser = vi.fn();
  const clear = vi.fn();

  beforeEach(() => {
    status.set('unknown');
    loadCurrentUser.mockReset();
    clear.mockReset();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            status,
            isAuthenticated: () => status() === 'authenticated',
            loadCurrentUser,
            clear,
          },
        },
      ],
    });
  });

  async function run(guard: CanActivateFn, url = '/assets'): Promise<boolean | UrlTree> {
    const result = TestBed.runInInjectionContext(() =>
      guard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot),
    );
    return firstValueFrom(
      isObservable(result)
        ? (result as Observable<boolean | UrlTree>)
        : of(result as boolean | UrlTree),
    );
  }

  const serialize = (tree: boolean | UrlTree) =>
    tree instanceof UrlTree ? TestBed.inject(Router).serializeUrl(tree) : tree;

  it('authGuard permite el acceso con sesión activa', async () => {
    status.set('authenticated');

    expect(await run(authGuard)).toBe(true);
    expect(loadCurrentUser).not.toHaveBeenCalled();
  });

  it('authGuard redirige a /login conservando returnUrl cuando no hay sesión', async () => {
    status.set('anonymous');

    expect(serialize(await run(authGuard, '/assets/42'))).toBe('/login?returnUrl=%2Fassets%2F42');
  });

  it('authGuard consulta /auth/me cuando el estado es desconocido', async () => {
    loadCurrentUser.mockReturnValue(of({ id: '1' }));

    expect(await run(authGuard)).toBe(true);
    expect(loadCurrentUser).toHaveBeenCalledTimes(1);
  });

  it('authGuard trata un fallo de red como sin sesión', async () => {
    loadCurrentUser.mockReturnValue(
      new Observable((subscriber) => subscriber.error(new Error('sin red'))),
    );

    expect(serialize(await run(authGuard))).toContain('/login');
    expect(clear).toHaveBeenCalled();
  });

  it('guestGuard deja pasar a los visitantes', async () => {
    status.set('anonymous');

    expect(await run(guestGuard, '/login')).toBe(true);
  });

  it('guestGuard redirige a /assets si ya hay sesión', async () => {
    status.set('authenticated');

    expect(serialize(await run(guestGuard, '/login'))).toBe('/assets');
  });
});
