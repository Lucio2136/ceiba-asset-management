import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { ICellRendererAngularComp } from 'ag-grid-angular';
import { ICellRendererParams } from 'ag-grid-community';
import { LucideEye, LucidePencil } from '@lucide/angular';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { AssetSummary } from '../../../../shared/models/asset.models';

/*
 * Renderizadores de celda de AG Grid. Son componentes Angular normales: se instancian dentro
 * del contexto de inyección de la rejilla, por lo que `routerLink` y Material funcionan.
 */

abstract class AssetCellBase implements ICellRendererAngularComp {
  protected readonly asset = signal<AssetSummary | undefined>(undefined);

  agInit(params: ICellRendererParams<AssetSummary>): void {
    this.asset.set(params.data);
  }

  refresh(params: ICellRendererParams<AssetSummary>): boolean {
    this.asset.set(params.data);
    return true;
  }
}

/** Nombre del activo con enlace a su detalle. */
@Component({
  selector: 'ceiba-asset-name-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink],
  template: `@if (asset(); as a) {
    <a class="name-link" [routerLink]="['/assets', a.id]">{{ a.name }}</a>
  }`,
  styles: `
    .name-link {
      color: var(--mat-sys-primary);
      font-weight: 500;
      text-decoration: none;

      &:hover,
      &:focus-visible {
        text-decoration: underline;
      }
    }
  `,
})
export class AssetNameCellComponent extends AssetCellBase {}

/** Estado del activo como etiqueta de color. */
@Component({
  selector: 'ceiba-asset-status-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [StatusBadgeComponent],
  template: `@if (asset(); as a) {
    <ceiba-status-badge [status]="a.status" />
  }`,
})
export class AssetStatusCellComponent extends AssetCellBase {}

/** Acciones por fila: ver detalle y editar. */
@Component({
  selector: 'ceiba-asset-actions-cell',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatButtonModule, RouterLink, LucideEye, LucidePencil],
  template: `@if (asset(); as a) {
    <a
      matIconButton
      [routerLink]="['/assets', a.id]"
      [attr.aria-label]="'Ver detalle de ' + a.name"
    >
      <svg lucideEye></svg>
    </a>
    <a
      matIconButton
      [routerLink]="['/assets', a.id, 'edit']"
      [attr.aria-label]="'Editar ' + a.name"
    >
      <svg lucidePencil></svg>
    </a>
  }`,
  styles: `
    :host {
      display: inline-flex;
      align-items: center;
    }
  `,
})
export class AssetActionsCellComponent extends AssetCellBase {}
