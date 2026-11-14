import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { AgGridAngular } from 'ag-grid-angular';
import {
  AllCommunityModule,
  ColDef,
  GetRowIdParams,
  GridOptions,
  ModuleRegistry,
  SortChangedEvent,
  themeQuartz,
} from 'ag-grid-community';
import { ASSET_TYPE_LABELS, AssetSummary, AssetType } from '../../../../shared/models/asset.models';
import { SortDirection } from '../../../../shared/models/common.models';
import {
  AssetActionsCellComponent,
  AssetNameCellComponent,
  AssetStatusCellComponent,
} from './asset-table-cells.component';

// AG Grid Community (MIT). No se usa ningún módulo Enterprise ni Server-Side Row Model:
// la paginación, el filtro y el orden los resuelve la API y la rejilla solo pinta la página.
ModuleRegistry.registerModules([AllCommunityModule]);

export interface AssetSort {
  sortBy?: string;
  sortDirection?: SortDirection;
}

/** Colores y tipografía del tema Material (`--mat-sys-*`) aplicados al tema Quartz de AG Grid. */
const gridTheme = themeQuartz.withParams({
  accentColor: 'var(--mat-sys-primary)',
  backgroundColor: 'var(--mat-sys-surface)',
  foregroundColor: 'var(--mat-sys-on-surface)',
  borderColor: 'var(--mat-sys-outline-variant)',
  headerBackgroundColor: 'var(--mat-sys-surface-container)',
  headerTextColor: 'var(--mat-sys-on-surface)',
  rowHoverColor: 'var(--mat-sys-surface-container-low)',
  fontFamily: 'inherit',
  browserColorScheme: 'light',
});

const LOCALE_TEXT: Record<string, string> = {
  loadingOoo: 'Cargando…',
  noRowsToShow: 'Sin filas para mostrar',
  ariaAscending: 'ascendente',
  ariaDescending: 'descendente',
  ariaUnsortedColumn: 'sin orden',
  ariaColumnHeader: 'Encabezado de columna',
};

// El servidor ya devuelve las filas ordenadas; el comparador nulo evita reordenar en el cliente.
const serverSorted: Pick<ColDef<AssetSummary>, 'sortable' | 'comparator' | 'unSortIcon'> = {
  sortable: true,
  comparator: () => 0,
  unSortIcon: true,
};

/**
 * Inventario de activos con AG Grid Community y modelo de filas del cliente.
 * Solo contiene la página actual: el filtro, el orden y la paginación viven en el servidor.
 */
@Component({
  selector: 'ceiba-asset-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AgGridAngular],
  template: `
    <ag-grid-angular
      class="grid"
      role="region"
      aria-label="Inventario de activos"
      [theme]="theme"
      [rowData]="rows()"
      [columnDefs]="columnDefs()"
      [defaultColDef]="defaultColDef"
      [gridOptions]="gridOptions"
      [loading]="loading()"
      [localeText]="localeText"
      [getRowId]="getRowId"
      domLayout="autoHeight"
      (sortChanged)="onSortChanged($event)"
    />
  `,
  styles: `
    .grid {
      display: block;
      width: 100%;
    }
  `,
})
export class AssetTableComponent {
  readonly rows = input.required<AssetSummary[]>();
  readonly loading = input(false);
  /** Orden inicial al montar la tabla; los cambios posteriores los dispara el usuario. */
  readonly initialSort = input<AssetSort>({});

  readonly sortChange = output<AssetSort>();

  protected readonly theme = gridTheme;
  protected readonly localeText = LOCALE_TEXT;
  protected readonly getRowId = (params: GetRowIdParams<AssetSummary>) => params.data.id;

  protected readonly gridOptions: GridOptions<AssetSummary> = {
    suppressMovableColumns: true,
    suppressCellFocus: false,
    tooltipShowDelay: 500,
  };

  protected readonly defaultColDef: ColDef<AssetSummary> = {
    resizable: true,
    suppressHeaderMenuButton: true,
  };

  // `initialSort` solo se aplica la primera vez; AG Grid conserva el orden del usuario después.
  protected readonly columnDefs = computed(() => this.buildColumns(this.initialSort()));

  protected onSortChanged(event: SortChangedEvent<AssetSummary>): void {
    const sorted = event.api.getColumnState().find((column) => column.sort);
    this.sortChange.emit(
      sorted
        ? { sortBy: sorted.colId, sortDirection: sorted.sort === 'desc' ? 'desc' : 'asc' }
        : {},
    );
  }

  private buildColumns(initial: AssetSort): ColDef<AssetSummary>[] {
    const sortOf = (colId: string) =>
      initial.sortBy === colId ? (initial.sortDirection ?? 'asc') : undefined;
    const dash = (value: string | null | undefined) => value || '—';

    return [
      {
        colId: 'name',
        headerName: 'Nombre',
        field: 'name',
        flex: 2,
        minWidth: 180,
        cellRenderer: AssetNameCellComponent,
        initialSort: sortOf('name'),
        ...serverSorted,
      },
      {
        colId: 'assetType',
        headerName: 'Tipo',
        field: 'assetType',
        flex: 1,
        minWidth: 140,
        valueFormatter: (p) => ASSET_TYPE_LABELS[p.value as AssetType] ?? String(p.value ?? ''),
        initialSort: sortOf('assetType'),
        ...serverSorted,
      },
      {
        colId: 'status',
        headerName: 'Estado',
        field: 'status',
        flex: 1,
        minWidth: 160,
        cellRenderer: AssetStatusCellComponent,
        initialSort: sortOf('status'),
        ...serverSorted,
      },
      {
        colId: 'serialNumber',
        headerName: 'Serie',
        field: 'serialNumber',
        flex: 1,
        minWidth: 140,
        valueFormatter: (p) => dash(p.value),
        initialSort: sortOf('serialNumber'),
        ...serverSorted,
      },
      {
        colId: 'locationName',
        headerName: 'Ubicación',
        field: 'locationName',
        flex: 1.5,
        minWidth: 160,
        valueFormatter: (p) => dash(p.value),
        initialSort: sortOf('locationName'),
        ...serverSorted,
      },
      {
        colId: 'assignedToName',
        headerName: 'Asignado a',
        field: 'assignedToName',
        flex: 1.5,
        minWidth: 160,
        valueFormatter: (p) => dash(p.value),
        initialSort: sortOf('assignedToName'),
        ...serverSorted,
      },
      {
        colId: 'actions',
        headerName: 'Acciones',
        width: 120,
        sortable: false,
        resizable: false,
        cellRenderer: AssetActionsCellComponent,
      },
    ];
  }
}
