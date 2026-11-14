import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ResponsibleSelectorComponent } from '../../../people/components/responsible-selector.component';
import { AssignAssetRequest } from '../../../../shared/models/asset.models';

export interface AssignDialogData {
  assetId: string;
  assetName: string;
}

/** Elige el responsable de un activo. Cierra con un `AssignAssetRequest` o sin valor. */
@Component({
  selector: 'ceiba-assign-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    ResponsibleSelectorComponent,
  ],
  template: `
    <h2 mat-dialog-title>Asignar activo</h2>
    <form [formGroup]="form" (ngSubmit)="submit()">
      <mat-dialog-content class="content">
        <p>
          Elige quién será responsable de <strong>{{ data.assetName }}</strong>.
        </p>
        <ceiba-responsible-selector formControlName="responsiblePersonId" [required]="true" />
        <mat-form-field>
          <mat-label>Notas (opcional)</mat-label>
          <textarea matInput formControlName="notes" rows="3" maxlength="500"></textarea>
        </mat-form-field>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button matButton type="button" mat-dialog-close>Cancelar</button>
        <button matButton="filled" type="submit" [disabled]="form.invalid">Asignar</button>
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
export class AssignDialogComponent {
  protected readonly data = inject<AssignDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef =
    inject<MatDialogRef<AssignDialogComponent, AssignAssetRequest>>(MatDialogRef);

  protected readonly form = inject(FormBuilder).group({
    responsiblePersonId: [null as string | null, [Validators.required]],
    notes: [''],
  });

  protected submit(): void {
    const { responsiblePersonId, notes } = this.form.getRawValue();
    if (this.form.invalid || !responsiblePersonId) {
      return;
    }
    this.dialogRef.close({
      assetId: this.data.assetId,
      responsiblePersonId,
      notes: notes?.trim() || null,
    });
  }
}
