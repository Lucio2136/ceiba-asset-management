import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ASSET_STATUS_LABELS, AssetStatus } from '../../models/asset.models';

/** Etiqueta de color para el estado de un activo. El color nunca es el único indicador. */
@Component({
  selector: 'ceiba-status-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-status]="status()">
    <span class="dot" aria-hidden="true"></span>{{ label() }}
  </span>`,
  styleUrl: './status-badge.component.scss',
})
export class StatusBadgeComponent {
  readonly status = input.required<AssetStatus>();

  protected readonly label = computed(() => ASSET_STATUS_LABELS[this.status()] ?? this.status());
}
