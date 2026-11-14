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
import { Location } from '../../../shared/models/location.models';
import { LocationsApi } from '../data/locations.api';

interface LocationGroup {
  buildingName: string;
  locations: Location[];
}

/**
 * Selector de ubicación compatible con formularios reactivos (`formControl`/`ngModel`).
 * Agrupa las opciones por edificio y muestra el piso.
 */
@Component({
  selector: 'ceiba-location-selector',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [MatFormFieldModule, MatSelectModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => LocationSelectorComponent),
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
        @if (allowEmpty()) {
          <mat-option [value]="null">Sin ubicación</mat-option>
        }
        @for (group of groups(); track group.buildingName) {
          <mat-optgroup [label]="group.buildingName">
            @for (location of group.locations; track location.id) {
              <mat-option [value]="location.id"
                >Piso {{ location.levelNumber }} · {{ location.name }}</mat-option
              >
            }
          </mat-optgroup>
        }
      </mat-select>
      @if (locations.isLoading()) {
        <mat-hint>Cargando ubicaciones…</mat-hint>
      } @else if (locations.error()) {
        <mat-hint role="alert">No se pudieron cargar las ubicaciones.</mat-hint>
      }
    </mat-form-field>
  `,
  styles: `
    .selector {
      width: 100%;
    }
  `,
})
export class LocationSelectorComponent implements ControlValueAccessor {
  private readonly api = inject(LocationsApi);

  readonly label = input('Ubicación');
  readonly required = input(false);
  /** Muestra la opción «Sin ubicación» (valor `null`). */
  readonly allowEmpty = input(false);

  protected readonly locations = rxResource({ stream: () => this.api.list() });
  protected readonly value = signal<string | null>(null);
  protected readonly disabled = signal(false);

  protected readonly groups = computed<LocationGroup[]>(() => {
    const byBuilding = new Map<string, Location[]>();
    for (const location of this.locations.hasValue() ? this.locations.value() : []) {
      const list = byBuilding.get(location.buildingName) ?? [];
      list.push(location);
      byBuilding.set(location.buildingName, list);
    }
    return [...byBuilding].map(([buildingName, locations]) => ({
      buildingName,
      locations: locations.sort(
        (a, b) => a.levelNumber - b.levelNumber || a.name.localeCompare(b.name, 'es'),
      ),
    }));
  });

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
