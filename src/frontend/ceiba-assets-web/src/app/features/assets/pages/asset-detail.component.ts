import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import {
  LucideArrowLeft,
  LucideArrowRightLeft,
  LucidePencil,
  LucideUndo2,
  LucideUserPlus,
} from '@lucide/angular';
import { Observable } from 'rxjs';
import {
  ConfirmDialogComponent,
  ConfirmDialogData,
} from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import {
  ASSET_IDENTIFIER_KIND_LABELS,
  ASSET_TYPE_LABELS,
  ASSIGNMENT_EVENT_LABELS,
  AssetDetail,
  AssetIdentifierKind,
  AssignAssetRequest,
  AssignmentEventKind,
  MoveAssetRequest,
} from '../../../shared/models/asset.models';
import { NotificationService } from '../../../shared/services/notification.service';
import {
  AssignDialogComponent,
  AssignDialogData,
} from '../components/assign-dialog/assign-dialog.component';
import {
  MoveDialogComponent,
  MoveDialogData,
} from '../components/move-dialog/move-dialog.component';
import { AssetsApi } from '../data/assets.api';

/** Detalle de un activo con las acciones de asignación: Asignar, Devolver y Mover. */
@Component({
  selector: 'ceiba-asset-detail',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DatePipe,
    EmptyStateComponent,
    ErrorStateComponent,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
    RouterLink,
    StatusBadgeComponent,
    LucideArrowLeft,
    LucideArrowRightLeft,
    LucidePencil,
    LucideUndo2,
    LucideUserPlus,
  ],
  templateUrl: './asset-detail.component.html',
  styleUrl: './asset-detail.component.scss',
})
export class AssetDetailComponent {
  private readonly api = inject(AssetsApi);
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  /** Identificador de la ruta `assets/:id`. */
  readonly id = input.required<string>();

  protected readonly typeLabels = ASSET_TYPE_LABELS;
  protected readonly historyColumns = ['when', 'kind', 'responsible', 'location', 'notes'];

  protected identifierLabel(kind: AssetIdentifierKind): string {
    return ASSET_IDENTIFIER_KIND_LABELS[kind] ?? kind;
  }

  protected readonly asset = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.api.get(params),
  });

  protected readonly history = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.api.history(params),
  });

  /** `true` mientras se ejecuta una acción; deshabilita los botones para evitar dobles envíos. */
  protected readonly busy = signal(false);

  protected readonly status = computed(() =>
    this.asset.hasValue() ? this.asset.value().status : null,
  );
  protected readonly canAssign = computed(() => this.status() === 'Available');
  protected readonly canReturn = computed(() => this.status() === 'Assigned');
  protected readonly canMove = computed(
    () => this.status() !== null && this.status() !== 'Retired',
  );

  protected eventLabel(kind: AssignmentEventKind): string {
    return ASSIGNMENT_EVENT_LABELS[kind] ?? kind;
  }

  protected assign(asset: AssetDetail): void {
    this.dialog
      .open<AssignDialogComponent, AssignDialogData, AssignAssetRequest>(AssignDialogComponent, {
        data: { assetId: asset.id, assetName: asset.name },
        width: '480px',
      })
      .afterClosed()
      .subscribe((request) => {
        if (request) {
          this.run(this.api.assign(request), 'Activo asignado.');
        }
      });
  }

  protected move(asset: AssetDetail): void {
    this.dialog
      .open<MoveDialogComponent, MoveDialogData, MoveAssetRequest>(MoveDialogComponent, {
        data: { assetId: asset.id, assetName: asset.name, currentLocationId: asset.locationId },
        width: '480px',
      })
      .afterClosed()
      .subscribe((request) => {
        if (request) {
          this.run(this.api.move(request), 'Activo movido.');
        }
      });
  }

  protected returnAsset(asset: AssetDetail): void {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: 'Devolver activo',
          message: `¿Confirmas la devolución de «${asset.name}»? Quedará disponible y sin responsable.`,
          confirmLabel: 'Devolver',
        },
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          this.run(this.api.return({ assetId: asset.id }), 'Activo devuelto.');
        }
      });
  }

  private run(operation: Observable<unknown>, successMessage: string): void {
    this.busy.set(true);
    operation.subscribe({
      next: () => {
        this.busy.set(false);
        this.notifications.success(successMessage);
        this.asset.reload();
        this.history.reload();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.notifications.error(error);
      },
    });
  }
}
