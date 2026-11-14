import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ShellComponent } from './shell.component';

describe('ShellComponent', () => {
  async function render(): Promise<HTMLElement> {
    await TestBed.configureTestingModule({
      imports: [ShellComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    const fixture = TestBed.createComponent(ShellComponent);
    await fixture.whenStable();
    return fixture.nativeElement as HTMLElement;
  }

  it('muestra la marca y la navegación principal', async () => {
    const element = await render();

    expect(element.querySelector('.brand-name')?.textContent).toContain('CEIBA Asset Management');
    expect(element.querySelector('nav[aria-label="Navegación principal"]')?.textContent).toContain(
      'Inicio',
    );
  });

  it('ofrece un enlace para saltar al contenido principal', async () => {
    const element = await render();

    const skipLink = element.querySelector<HTMLAnchorElement>('.skip-link');
    expect(skipLink?.getAttribute('href')).toBe('#main-content');
    expect(element.querySelector('main#main-content')).not.toBeNull();
  });
});
