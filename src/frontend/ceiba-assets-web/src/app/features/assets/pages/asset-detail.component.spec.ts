import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AssetDetail, AssetStatus } from '../../../shared/models/asset.models';
import { AssetsApi } from '../data/assets.api';
import { AssetDetailComponent } from './asset-detail.component';

const detail = (status: AssetStatus): AssetDetail => ({
  id: '7',
  name: 'Laptop Ana',
  assetType: 'Laptop',
  status,
  manufacturer: 'Lenovo',
  model: 'ThinkPad T14',
  serialNumber: 'SN-1',
  locationId: 'loc-1',
  locationName: 'Área abierta',
  locationPath: 'Torre A / Piso 3 / Área abierta',
  notes: null,
  createdAtUtc: '2026-11-01T12:00:00Z',
  updatedAtUtc: '2026-11-01T12:00:00Z',
  rowVersion: 'AAAA',
  identifiers: [{ id: 'i-1', kind: 'SerialNumber', value: 'SN-1', isPrimary: true }],
  currentAssignment:
    status === 'Assigned'
      ? {
          id: 'as-1',
          assetId: '7',
          responsiblePersonId: 'p-1',
          responsiblePersonName: 'Ana Pérez',
          assignedAtUtc: '2026-11-01T12:00:00Z',
          notes: null,
        }
      : null,
});

describe('AssetDetailComponent', () => {
  async function render(status: AssetStatus, dialogResult: unknown = undefined) {
    const api = {
      get: vi.fn(() => of(detail(status))),
      history: vi.fn(() => of([])),
      assign: vi.fn(() => of({})),
      return: vi.fn(() => of({})),
      move: vi.fn(() => of({})),
    };
    const dialog = { open: vi.fn(() => ({ afterClosed: () => of(dialogResult) })) };
    await TestBed.configureTestingModule({
      imports: [AssetDetailComponent],
      providers: [
        provideRouter([]),
        { provide: AssetsApi, useValue: api },
        { provide: MatDialog, useValue: dialog },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetDetailComponent);
    fixture.componentRef.setInput('id', '7');
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const button = (label: string) =>
      Array.from(element.querySelectorAll('button')).find((node) =>
        node.textContent?.includes(label),
      );

    return { fixture, api, button, element };
  }

  it('un activo disponible ofrece Asignar y Mover', async () => {
    const { button } = await render('Available');

    expect(button('Asignar')).toBeDefined();
    expect(button('Mover')).toBeDefined();
    expect(button('Devolver')).toBeUndefined();
  });

  it('un activo asignado ofrece Devolver y Mover', async () => {
    const { button } = await render('Assigned');

    expect(button('Devolver')).toBeDefined();
    expect(button('Mover')).toBeDefined();
    expect(button('Asignar')).toBeUndefined();
  });

  it('un activo dado de baja no ofrece acciones de asignación', async () => {
    const { button } = await render('Retired');

    expect(button('Asignar')).toBeUndefined();
    expect(button('Devolver')).toBeUndefined();
    expect(button('Mover')).toBeUndefined();
  });

  it('asigna el activo con el responsable elegido en el diálogo y recarga los datos', async () => {
    const { fixture, api, button } = await render('Available', {
      assetId: '7',
      responsiblePersonId: 'p-1',
      notes: null,
    });

    button('Asignar')?.click();
    await fixture.whenStable();

    expect(api.assign).toHaveBeenCalledWith({
      assetId: '7',
      responsiblePersonId: 'p-1',
      notes: null,
    });
    expect(api.get).toHaveBeenCalledTimes(2);
  });

  it('no llama a la API si se cancela el diálogo', async () => {
    const { fixture, api, button } = await render('Available', undefined);

    button('Asignar')?.click();
    await fixture.whenStable();

    expect(api.assign).not.toHaveBeenCalled();
  });

  it('devuelve el activo tras confirmar', async () => {
    const { fixture, api, button } = await render('Assigned', true);

    button('Devolver')?.click();
    await fixture.whenStable();

    expect(api.return).toHaveBeenCalledWith({ assetId: '7' });
  });
});
