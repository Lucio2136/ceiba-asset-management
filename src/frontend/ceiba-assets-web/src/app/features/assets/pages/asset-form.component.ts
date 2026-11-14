import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { LucideArrowLeft, LucideSave } from '@lucide/angular';
import { LocationSelectorComponent } from '../../locations/components/location-selector.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import {
  ASSET_TYPE_LABELS,
  ASSET_TYPES,
  AssetType,
  SaveAssetRequest,
} from '../../../shared/models/asset.models';
import { NotificationService } from '../../../shared/services/notification.service';
import { AssetsApi } from '../data/assets.api';

/** Alta (`/assets/new`) y edición (`/assets/:id/edit`) de un activo. */
@Component({
  selector: 'ceiba-asset-form',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ErrorStateComponent,
    LocationSelectorComponent,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    ReactiveFormsModule,
    RouterLink,
    LucideArrowLeft,
    LucideSave,
  ],
  templateUrl: './asset-form.component.html',
  styleUrl: './asset-form.component.scss',
})
export class AssetFormComponent {
  private readonly api = inject(AssetsApi);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  /** Identificador de la ruta `assets/:id/edit`; ausente en el alta. */
  readonly id = input<string>();

  protected readonly isEdit = computed(() => this.id() !== undefined);
  protected readonly saving = signal(false);
  protected readonly typeOptions = ASSET_TYPES.map((value) => ({
    value,
    label: ASSET_TYPE_LABELS[value],
  }));

  protected readonly asset = rxResource({
    params: () => this.id(),
    stream: ({ params }) => this.api.get(params),
  });

  private rowVersion: string | undefined;
  protected readonly currentLocationLabel = signal<string | null>(null);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    assetType: [null as AssetType | null, [Validators.required]],
    manufacturer: ['', [Validators.maxLength(100)]],
    model: ['', [Validators.maxLength(100)]],
    serialNumber: ['', [Validators.maxLength(100)]],
    locationId: [null as string | null],
    notes: ['', [Validators.maxLength(2000)]],
  });

  constructor() {
    effect(() => {
      if (!this.asset.hasValue()) {
        return;
      }
      const asset = this.asset.value();
      this.rowVersion = asset.rowVersion;
      this.currentLocationLabel.set(asset.locationPath || asset.locationName || null);
      this.form.reset({
        name: asset.name,
        assetType: asset.assetType,
        manufacturer: asset.manufacturer ?? '',
        model: asset.model ?? '',
        serialNumber: asset.serialNumber ?? '',
        locationId: asset.locationId ?? null,
        notes: asset.notes ?? '',
      });
    });
  }

  protected save(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: SaveAssetRequest = {
      name: value.name.trim(),
      assetType: value.assetType as AssetType,
      manufacturer: value.manufacturer?.trim() || null,
      model: value.model?.trim() || null,
      serialNumber: value.serialNumber?.trim() || null,
      notes: value.notes?.trim() || null,
      locationId: this.isEdit() ? undefined : value.locationId,
      rowVersion: this.rowVersion,
    };

    const id = this.id();
    const call = id ? this.api.update(id, request) : this.api.create(request);

    this.saving.set(true);
    call.subscribe({
      next: (saved) => {
        this.notifications.success(id ? 'Activo actualizado.' : 'Activo creado.');
        void this.router.navigate(['/assets', saved.id]);
      },
      error: (error: unknown) => {
        this.saving.set(false);
        this.notifications.error(error);
      },
    });
  }
}
