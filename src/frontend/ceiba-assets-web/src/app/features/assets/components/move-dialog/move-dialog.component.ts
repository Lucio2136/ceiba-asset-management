import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { LocationSelectorComponent } from '../../../locations/components/location-selector.component';
import { MoveAssetRequest } from '../../../../shared/models/asset.models';

export interface MoveDialogData {
  assetId: string;
  assetName: string;
  currentLocationId?: string | null;
}

/** Elige la nueva ubicación de un activo. Cierra con un `MoveAssetRequest` o sin valor. */
@Component({
  selector: 'ceiba-move-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    LocationSelectorComponent,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
  ],
  template: `
    <h2 mat-dialog-title>Mover activo</h2>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <mat-dialog-content class="content">
        <p>
          Elige la nueva ubicación de <strong>{{ data.assetName }}</strong>.
        </p>
        <ceiba-location-selector
          formControlName="toLocationId"
          label="Nueva ubicación"
          [required]="true"
        />
        <mat-form-field>
          <mat-label>Motivo (opcional)</mat-label>
          <textarea matInput formControlName="reason" rows="3" maxlength="500"></textarea>
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button matButton type="button" mat-dialog-close>Cancelar</button>
        <button matButton="filled" type="submit" [disabled]="form.invalid">Mover</button>
      </mat-dialog-actions>
    </form>
  `,
  styles: `
    .content {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }
  `,
})
export class MoveDialogComponent {
  protected readonly data = inject<MoveDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef =
    inject<MatDialogRef<MoveDialogComponent, MoveAssetRequest>>(MatDialogRef);

  protected readonly form = inject(FormBuilder).group({
    toLocationId: [
      null as string | null,
      [
        Validators.required,
        (control: { value: string | null }) =>
          control.value && control.value === this.data.currentLocationId
            ? { sameLocation: true }
            : null,
      ],
    ],
    reason: [''],
  });

  protected submit(): void {
    const { toLocationId, reason } = this.form.getRawValue();
    if (this.form.invalid || !toLocationId) {
      return;
    }
    this.dialogRef.close({
      assetId: this.data.assetId,
      toLocationId,
      reason: reason?.trim() || null,
    });
  }
}
