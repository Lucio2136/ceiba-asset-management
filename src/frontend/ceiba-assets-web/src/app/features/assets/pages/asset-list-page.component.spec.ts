import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { AssetListQuery, AssetSummary } from '../../../shared/models/asset.models';
import { PagedResult } from '../../../shared/models/common.models';
import { AssetTableComponent } from '../components/asset-table/asset-table.component';
import { AssetsApi } from '../data/assets.api';
import { AssetListPageComponent } from './asset-list-page.component';

/** Evita montar AG Grid en jsdom: aquí se prueba la orquestación de la página. */
@Component({
  selector: 'ceiba-asset-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<ul data-testid="rows">
    @for (row of rows(); track row.id) {
      <li>{{ row.name }}</li>
    }
  </ul>`,
})
class AssetTableStubComponent {
  readonly rows = input.required<AssetSummary[]>();
  readonly loading = input(false);
  readonly initialSort = input({});
}

const asset = (id: string, name: string): AssetSummary => ({
  id,
  name,
  assetType: 'Laptop',
  status: 'Available',
});

describe('AssetListPageComponent', () => {
  async function render(list: (query: AssetListQuery) => Observable<PagedResult<AssetSummary>>) {
    const listSpy = vi.fn(list);
    TestBed.configureTestingModule({
      imports: [AssetListPageComponent],
      providers: [provideRouter([]), { provide: AssetsApi, useValue: { list: listSpy } }],
    });
    TestBed.overrideComponent(AssetListPageComponent, {
      remove: { imports: [AssetTableComponent] },
      add: { imports: [AssetTableStubComponent] },
    });
    await TestBed.compileComponents();

    const fixture = TestBed.createComponent(AssetListPageComponent);
    await fixture.whenStable();
    return { fixture, listSpy, element: fixture.nativeElement as HTMLElement };
  }

  it('pide al servidor la primera página y muestra las filas', async () => {
    const { listSpy, element } = await render(() =>
      of({
        items: [asset('1', 'Laptop Ana'), asset('2', 'Monitor Luis')],
        totalCount: 2,
        page: 1,
        pageSize: 25,
      }),
    );

    expect(listSpy).toHaveBeenCalledWith(expect.objectContaining({ page: 1, pageSize: 25 }));
    const rows = Array.from(element.querySelectorAll('[data-testid="rows"] li')).map(
      (li) => li.textContent,
    );
    expect(rows).toEqual(['Laptop Ana', 'Monitor Luis']);
  });

  it('muestra el estado vacío cuando no hay activos', async () => {
    const { element } = await render(() => of({ items: [], totalCount: 0, page: 1, pageSize: 25 }));

    expect(element.textContent).toContain('Aún no hay activos');
  });

  it('muestra un error recuperable cuando la API falla', async () => {
    const { element } = await render(() =>
      throwError(() => new HttpErrorResponse({ status: 503 })),
    );

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Error del servidor');
    expect(element.textContent).toContain('Reintentar');
  });

  it('muestra un estado de no autorizado cuando la API responde 403', async () => {
    const { element } = await render(() =>
      throwError(() => new HttpErrorResponse({ status: 403 })),
    );

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Acceso denegado');
    expect(element.textContent).not.toContain('Reintentar');
  });
});
