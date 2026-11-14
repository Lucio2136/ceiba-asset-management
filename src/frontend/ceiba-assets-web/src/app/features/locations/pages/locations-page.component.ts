import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { BuildingsApi } from '../../buildings/data/buildings.api';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { LocationsApi } from '../data/locations.api';

/**
 * Ubicaciones filtradas por edificio y piso. La URL (`?buildingId=&floor=`) es la fuente de
 * verdad, así los filtros se pueden compartir y los enlaces de la página de edificios funcionan.
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
  readonly floor = input<string>();

  protected readonly columns = ['building', 'floor', 'name', 'code'];

  protected readonly buildings = rxResource({ stream: () => this.buildingsApi.list() });

  protected readonly buildingList = computed(() =>
    this.buildings.hasValue() ? this.buildings.value() : [],
  );

  protected readonly floorNumber = computed(() => {
    const raw = this.floor();
    const parsed = raw === undefined || raw === '' ? NaN : Number(raw);
    return Number.isInteger(parsed) ? parsed : undefined;
  });

  protected readonly floorOptions = computed(() => {
    const building = this.buildingList().find((b) => b.id === this.buildingId());
    return building ? Array.from({ length: building.levelCount }, (_, i) => i + 1) : [];
  });

  protected readonly locations = rxResource({
    params: () => ({ buildingId: this.buildingId() || undefined, floor: this.floorNumber() }),
    stream: ({ params }) => this.locationsApi.list(params),
  });

  protected selectBuilding(buildingId: string): void {
    // Cambiar de edificio invalida el piso seleccionado.
    void this.router.navigate([], {
      queryParams: { buildingId: buildingId || null, floor: null },
      queryParamsHandling: 'merge',
    });
  }

  protected selectFloor(floor: number | ''): void {
    void this.router.navigate([], {
      queryParams: { floor: floor === '' ? null : floor },
      queryParamsHandling: 'merge',
    });
  }
}
