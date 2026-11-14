import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  LucideCircleCheck,
  LucideCircleExclamationPoint,
  LucideRefreshCw,
  LucideServer,
} from '@lucide/angular';
import { PlatformStatusService } from '../../services/platform-status.service';

/**
 * Página de inicio. Se sustituirá por el dashboard cuando exista esa funcionalidad.
 */
@Component({
  selector: 'ceiba-home-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    LucideCircleCheck,
    LucideCircleExclamationPoint,
    LucideRefreshCw,
    LucideServer,
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
})
export class HomePageComponent {
  private readonly platformStatus = inject(PlatformStatusService);

  protected readonly systemInfo = rxResource({
    stream: () => this.platformStatus.getSystemInfo(),
  });
}
