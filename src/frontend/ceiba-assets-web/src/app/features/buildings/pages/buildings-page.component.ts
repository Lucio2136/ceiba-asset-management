import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { LucideMapPin } from '@lucide/angular';
import { forkJoin, map, of, switchMap } from 'rxjs';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { Building, Floor } from '../../../shared/models/location.models';
import { BuildingsApi } from '../data/buildings.api';

export interface BuildingWithFloors extends Building {
  floors: Floor[];
}

@Component({
  selector: 'ceiba-buildings-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    EmptyStateComponent,
    ErrorStateComponent,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule,
    RouterLink,
    LucideMapPin,
  ],
  templateUrl: './buildings-page.component.html',
  styleUrl: './buildings-page.component.scss',
})
export class BuildingsPageComponent {
  private readonly api = inject(BuildingsApi);

  protected readonly columns = ['name', 'floors', 'actions'];

  protected floorsLabel(count: number): string {
    return count === 1 ? '1 piso' : `${count} pisos`;
  }

  protected readonly buildings = rxResource({
    stream: () =>
      this.api.list().pipe(
        switchMap((items) => {
          if (items.length === 0) {
            return of([] as BuildingWithFloors[]);
          }
          return forkJoin(
            items.map((building) =>
              this.api.floors(building.id).pipe(
                map((floors) => ({
                  ...building,
                  floors: [...floors].sort((a, b) => a.levelNumber - b.levelNumber),
                })),
              ),
            ),
          );
        }),
      ),
  });
}
