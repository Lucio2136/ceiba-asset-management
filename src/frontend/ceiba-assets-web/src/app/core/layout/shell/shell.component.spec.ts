import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { CurrentUser } from '../../../shared/models/auth.models';
import { AuthService } from '../../authentication/auth.service';
import { ShellComponent } from './shell.component';

const user: CurrentUser = {
  id: '1',
  email: 'ana@ceiba.example',
  displayName: 'Ana Pérez',
  roles: ['Admin'],
};

describe('ShellComponent', () => {
  async function render(current: CurrentUser | null) {
    const userSignal = signal(current);
    const logout = vi.fn(() => of(undefined));
    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            user: userSignal,
            isAuthenticated: () => userSignal() !== null,
            logout,
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ShellComponent);
    await fixture.whenStable();
    return { element: fixture.nativeElement as HTMLElement, fixture, logout };
  }

  it('muestra la marca, la navegación principal y el usuario con sesión iniciada', async () => {
    const { element } = await render(user);

    expect(element.querySelector('.brand-name')?.textContent).toContain('CEIBA Asset Management');
    const nav = element.querySelector('nav[aria-label="Navegación principal"]');
    for (const label of ['Activos', 'Edificios', 'Ubicaciones', 'Responsables']) {
      expect(nav?.textContent).toContain(label);
    }
    expect(element.querySelector('[data-testid="current-user"]')?.textContent).toContain(
      'Ana Pérez',
    );
  });

  it('oculta la navegación y el usuario sin sesión', async () => {
    const { element } = await render(null);

    expect(element.querySelector('nav[aria-label="Navegación principal"]')).toBeNull();
    expect(element.querySelector('[data-testid="current-user"]')).toBeNull();
    expect(element.textContent).not.toContain('Cerrar sesión');
  });

  it('cierra la sesión y navega a /login', async () => {
    const { element, fixture, logout } = await render(user);
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    element.querySelector<HTMLButtonElement>('button.logout')?.click();
    await fixture.whenStable();

    expect(logout).toHaveBeenCalledTimes(1);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });

  it('ofrece un enlace para saltar al contenido principal', async () => {
    const { element } = await render(user);

    const skipLink = element.querySelector<HTMLAnchorElement>('.skip-link');
    expect(skipLink?.getAttribute('href')).toBe('#main-content');
    expect(element.querySelector('main#main-content')).not.toBeNull();
  });
});
