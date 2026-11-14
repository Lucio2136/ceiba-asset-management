import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  linkedSignal,
  signal,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { LucidePlus } from '@lucide/angular';
import {
  DataFilterValue,
  DataFiltersComponent,
  SelectFilter,
} from '../../../shared/components/data-filters/data-filters.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import {
  ASSET_STATUS_LABELS,
  ASSET_STATUSES,
  ASSET_TYPE_LABELS,
  ASSET_TYPES,
  AssetListQuery,
  AssetStatus,
  AssetSummary,
  AssetType,
} from '../../../shared/models/asset.models';
import { PagedResult } from '../../../shared/models/common.models';
import { AssetSort, AssetTableComponent } from '../components/asset-table/asset-table.component';
import { AssetsApi } from '../data/assets.api';

const PAGE_SIZE_OPTIONS = [10, 25, 50, 100];

/** Inventario: filtros, orden y paginación se piden al servidor; la rejilla pinta la página. */
@Component({
  selector: 'ceiba-asset-list-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    AssetTableComponent,
    DataFiltersComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    MatButtonModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    RouterLink,
    LucidePlus,
  ],
  templateUrl: './asset-list-page.component.html',
  styleUrl: './asset-list-page.component.scss',
})
export class AssetListPageComponent {
  private readonly api = inject(AssetsApi);

  protected readonly pageSizeOptions = PAGE_SIZE_OPTIONS;

  protected readonly filters = signal<DataFilterValue>({ search: '', selections: {} });
  protected readonly sort = signal<AssetSort>({});
  protected readonly pageSize = signal(PAGE_SIZE_OPTIONS[1]);
  /** Página 1-based. Vuelve a 1 cuando cambian filtros, orden o tamaño de página. */
  protected readonly page = linkedSignal({
    source: () => ({ filters: this.filters(), sort: this.sort(), pageSize: this.pageSize() }),
    computation: () => 1,
  });

  protected readonly selects: readonly SelectFilter[] = [
    {
      key: 'type',
      label: 'Tipo',
      options: ASSET_TYPES.map((value) => ({ value, label: ASSET_TYPE_LABELS[value] })),
    },
    {
      key: 'status',
      label: 'Estado',
      options: ASSET_STATUSES.map((value) => ({ value, label: ASSET_STATUS_LABELS[value] })),
    },
  ];

  private readonly query = computed<AssetListQuery>(() => {
    const { search, selections } = this.filters();
    const { sortBy, sortDirection } = this.sort();
    return {
      page: this.page(),
      pageSize: this.pageSize(),
      search: search || undefined,
      type: (selections['type'] || undefined) as AssetType | undefined,
      status: (selections['status'] || undefined) as AssetStatus | undefined,
      sortBy,
      sortDirection,
    };
  });

  protected readonly assets = rxResource({
    params: () => this.query(),
    stream: ({ params }) => this.api.list(params),
  });

  /** Conserva la última página mientras llega la siguiente, para no destruir la rejilla. */
  protected readonly current = linkedSignal<
    PagedResult<AssetSummary> | undefined,
    PagedResult<AssetSummary> | undefined
  >({
    source: () => (this.assets.hasValue() ? this.assets.value() : undefined),
    computation: (value, previous) => value ?? previous?.value,
  });

  protected readonly hasActiveFilters = computed(() => {
    const { search, selections } = this.filters();
    return search !== '' || Object.values(selections).some((value) => value !== '');
  });

  protected onPage(event: PageEvent): void {
    if (event.pageSize !== this.pageSize()) {
      this.pageSize.set(event.pageSize);
    } else {
      this.page.set(event.pageIndex + 1);
    }
  }
}
