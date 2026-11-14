import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { LucideLock, LucideRefreshCw, LucideTriangleAlert } from '@lucide/angular';
import { describeError } from '../../utils/api-error';

/**
 * Error de carga (`role="alert"`). Distingue «no autorizado»/«prohibido» de los demás fallos:
 * en esos casos se ofrece volver a iniciar sesión en lugar de reintentar.
 */
@Component({
  selector: 'ceiba-error-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, RouterLink, LucideLock, LucideRefreshCw, LucideTriangleAlert],
  template: `
    <div class="state state-error" role="alert">
      @if (isAccessError()) {
        <svg lucideLock class="icon" aria-hidden="true"></svg>
      } @else {
        <svg lucideTriangleAlert class="icon" aria-hidden="true"></svg>
      }
      <h2 class="title">{{ described().title }}</h2>
      <p class="message">{{ described().message }}</p>
      <div class="actions">
        @if (described().kind === 'unauthorized') {
          <a matButton="filled" routerLink="/login">Iniciar sesión</a>
        } @else if (described().kind !== 'forbidden') {
          <button matButton="outlined" type="button" (click)="retry.emit()">
            <svg lucideRefreshCw></svg>
            Reintentar
          </button>
        }
      </div>
    </div>
  `,
  styleUrl: '../state-message.scss',
})
export class ErrorStateComponent {
  readonly error = input.required<unknown>();
  readonly retry = output<void>();

  protected readonly described = computed(() => describeError(this.error()));
  protected readonly isAccessError = computed(() =>
    ['unauthorized', 'forbidden'].includes(this.described().kind),
  );
}
