import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { LucideInbox } from '@lucide/angular';

/** Estado vacío: sin datos o sin resultados. El contenido proyectado son las acciones. */
@Component({
  selector: 'ceiba-empty-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [LucideInbox],
  template: `
    <div class="state" role="status">
      <svg lucideInbox class="icon" aria-hidden="true"></svg>
      <h2 class="title">{{ title() }}</h2>
      @if (message()) {
        <p class="message">{{ message() }}</p>
      }
      <div class="actions"><ng-content /></div>
    </div>
  `,
  styleUrl: '../state-message.scss',
})
export class EmptyStateComponent {
  readonly title = input.required<string>();
  readonly message = input<string>();
}
