import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { LucideMapPin } from '@lucide/angular';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { BuildingsApi } from '../data/buildings.api';

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

  protected readonly columns = ['name', 'code', 'floors', 'actions'];

  protected readonly buildings = rxResource({ stream: () => this.api.list() });

  /** Números de piso 1..N para enlazar a las ubicaciones de cada piso. */
  protected floorsOf(levelCount: number): number[] {
    return Array.from({ length: levelCount }, (_, index) => index + 1);
  }
}
