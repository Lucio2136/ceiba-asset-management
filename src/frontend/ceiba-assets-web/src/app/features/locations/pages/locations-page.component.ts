import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { BuildingsApi } from '../../buildings/data/buildings.api';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { LocationsApi } from '../data/locations.api';

/**
 * Ubicaciones filtradas por edificio y piso. La URL (`?buildingId=&floorId=`) es la fuente
 * de verdad para compartir filtros y enlaces desde la página de edificios.
 */
@Component({
  selector: 'ceiba-locations-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    EmptyStateComponent,
    ErrorStateComponent,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
  ],
  templateUrl: './locations-page.component.html',
  styleUrl: './locations-page.component.scss',
})
export class LocationsPageComponent {
  private readonly router = inject(Router);
  private readonly locationsApi = inject(LocationsApi);
  private readonly buildingsApi = inject(BuildingsApi);

  // Query params enlazados por `withComponentInputBinding`.
  readonly buildingId = input<string>();
  readonly floorId = input<string>();

  protected readonly columns = ['building', 'floor', 'name', 'code', 'description'];

  protected readonly buildings = rxResource({ stream: () => this.buildingsApi.list() });

  protected readonly buildingList = computed(() =>
    this.buildings.hasValue() ? this.buildings.value() : [],
  );

  protected readonly floors = rxResource({
    params: () => this.buildingId() || undefined,
    stream: ({ params }) => (params ? this.buildingsApi.floors(params) : of([])),
  });

  protected readonly floorOptions = computed(() =>
    this.floors.hasValue() ? this.floors.value() : [],
  );

  protected readonly locations = rxResource({
    params: () => ({
      buildingId: this.buildingId() || undefined,
      floorId: this.floorId() || undefined,
    }),
    stream: ({ params }) => this.locationsApi.list(params),
  });

  protected selectBuilding(buildingId: string): void {
    void this.router.navigate([], {
      queryParams: { buildingId: buildingId || null, floorId: null },
      queryParamsHandling: 'merge',
    });
  }

  protected selectFloor(floorId: string): void {
    void this.router.navigate([], {
      queryParams: { floorId: floorId || null },
      queryParamsHandling: 'merge',
    });
  }
}
