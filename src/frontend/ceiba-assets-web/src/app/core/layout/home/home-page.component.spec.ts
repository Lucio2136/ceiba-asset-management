import { TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';
import { SystemInfoResponse } from '../../http/generated/ceiba-api.client';
import { PlatformStatusService } from '../../services/platform-status.service';
import { HomePageComponent } from './home-page.component';

describe('HomePageComponent', () => {
  async function render(response: () => Observable<SystemInfoResponse>) {
    const getSystemInfo = vi.fn(response);
    await TestBed.configureTestingModule({
      imports: [HomePageComponent],
      providers: [{ provide: PlatformStatusService, useValue: { getSystemInfo } }],
    }).compileComponents();

    const fixture = TestBed.createComponent(HomePageComponent);
    await fixture.whenStable();
    return { fixture, getSystemInfo, element: fixture.nativeElement as HTMLElement };
  }

  it('muestra la versión cuando la API responde', async () => {
    const { element } = await render(() =>
      of({ applicationName: 'CEIBA Asset Management', version: '0.1.0' }),
    );

    expect(element.textContent).toContain('API operativa');
    expect(element.textContent).toContain('0.1.0');
  });

  it('muestra un error recuperable y permite reintentar', async () => {
    const { fixture, element, getSystemInfo } = await render(() =>
      throwError(() => new Error('Sin conexión')),
    );

    expect(element.querySelector('[role="alert"]')?.textContent).toContain(
      'No fue posible contactar la API',
    );

    element.querySelector<HTMLButtonElement>('button')?.click();
    await fixture.whenStable();

    expect(getSystemInfo).toHaveBeenCalledTimes(2);
  });
});
