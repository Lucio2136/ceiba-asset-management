import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { AuthService } from '../../../core/authentication/auth.service';
import { CurrentUser } from '../../../shared/models/auth.models';
import { LoginPageComponent } from './login-page.component';

const user: CurrentUser = {
  id: '1',
  email: 'admin@ceiba.example',
  displayName: 'Admin',
  roles: [],
};

describe('LoginPageComponent', () => {
  async function render(login: () => Observable<CurrentUser>, returnUrl?: string) {
    const loginSpy = vi.fn(login);
    await TestBed.configureTestingModule({
      imports: [LoginPageComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: { login: loginSpy } }],
    }).compileComponents();

    const router = TestBed.inject(Router);
    const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    const fixture = TestBed.createComponent(LoginPageComponent);
    if (returnUrl !== undefined) {
      fixture.componentRef.setInput('returnUrl', returnUrl);
    }
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const fill = async (userName: string, password: string) => {
      const [userInput, passwordInput] = Array.from(element.querySelectorAll('input'));
      userInput.value = userName;
      userInput.dispatchEvent(new Event('input'));
      passwordInput.value = password;
      passwordInput.dispatchEvent(new Event('input'));
      await fixture.whenStable();
    };
    const submit = async () => {
      element.querySelector('form')?.dispatchEvent(new Event('submit'));
      await fixture.whenStable();
    };

    return { fixture, element, loginSpy, navigate, fill, submit };
  }

  it('no llama a la API si el formulario es inválido y muestra los errores', async () => {
    const { element, loginSpy, submit } = await render(() => of(user));

    await submit();

    expect(loginSpy).not.toHaveBeenCalled();
    expect(element.textContent).toContain('Ingresa un correo válido.');
    expect(element.textContent).toContain('Ingresa tu contraseña.');
  });

  it('inicia sesión y navega a /assets por omisión', async () => {
    const { loginSpy, navigate, fill, submit } = await render(() => of(user));

    await fill('admin@ceiba.example', 'secreto');
    await submit();

    expect(loginSpy).toHaveBeenCalledWith({ email: 'admin@ceiba.example', password: 'secreto' });
    expect(navigate).toHaveBeenCalledWith('/assets');
  });

  it('respeta returnUrl interno e ignora destinos externos', async () => {
    const internal = await render(() => of(user), '/assets/42');
    await internal.fill('admin@ceiba.example', 'x');
    await internal.submit();
    expect(internal.navigate).toHaveBeenCalledWith('/assets/42');

    TestBed.resetTestingModule();

    const external = await render(() => of(user), '//evil.example.com');
    await external.fill('admin@ceiba.example', 'x');
    await external.submit();
    expect(external.navigate).toHaveBeenCalledWith('/assets');
  });

  it('muestra un mensaje cuando las credenciales son incorrectas', async () => {
    const { element, navigate, fill, submit } = await render(() =>
      throwError(() => new HttpErrorResponse({ status: 401 })),
    );

    await fill('admin@ceiba.example', 'mala');
    await submit();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain(
      'Usuario o contraseña incorrectos.',
    );
    expect(navigate).not.toHaveBeenCalled();
  });
});
