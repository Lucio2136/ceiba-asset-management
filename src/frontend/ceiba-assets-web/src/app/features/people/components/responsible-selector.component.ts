import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  inject,
  input,
  signal,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { PeopleApi } from '../data/people.api';

/** Selector de responsable compatible con formularios reactivos (`formControl`/`ngModel`). */
@Component({
  selector: 'ceiba-responsible-selector',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatFormFieldModule, MatSelectModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ResponsibleSelectorComponent),
      multi: true,
    },
  ],
  template: `
    <mat-form-field class="selector">
      <mat-label>{{ label() }}</mat-label>
      <mat-select
        [value]="value()"
        [disabled]="disabled()"
        [required]="required()"
        (selectionChange)="select($event.value)"
        (closed)="onTouched()"
      >
        @for (person of options(); track person.id) {
          <mat-option [value]="person.id">
            {{ person.displayName }}
            @if (person.department) {
              · {{ person.department }}
            }
          </mat-option>
        }
      </mat-select>
      @if (people.isLoading()) {
        <mat-hint>Cargando responsables…</mat-hint>
      } @else if (people.error()) {
        <mat-hint role="alert">No se pudieron cargar los responsables.</mat-hint>
      }
    </mat-form-field>
  `,
  styles: `
    .selector {
      width: 100%;
    }
  `,
})
export class ResponsibleSelectorComponent implements ControlValueAccessor {
  private readonly api = inject(PeopleApi);

  readonly label = input('Responsable');
  readonly required = input(false);

  protected readonly people = rxResource({ stream: () => this.api.list() });
  protected readonly options = computed(() => (this.people.hasValue() ? this.people.value() : []));
  protected readonly value = signal<string | null>(null);
  protected readonly disabled = signal(false);

  private onChange: (value: string | null) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  protected select(id: string | null): void {
    this.value.set(id);
    this.onChange(id);
  }

  writeValue(id: string | null): void {
    this.value.set(id ?? null);
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }
}
