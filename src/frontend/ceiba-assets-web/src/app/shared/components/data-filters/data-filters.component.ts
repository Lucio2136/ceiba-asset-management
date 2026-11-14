import { ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LucideSearch, LucideX } from '@lucide/angular';
import { debounceTime, distinctUntilChanged } from 'rxjs';

export interface FilterOption {
  value: string;
  label: string;
}

export interface SelectFilter {
  key: string;
  label: string;
  options: readonly FilterOption[];
}

/** `selections[key] === ''` significa «todos». */
export interface DataFilterValue {
  search: string;
  selections: Record<string, string>;
}

/** Barra de filtros: búsqueda con debounce y listas desplegables configurables. */
@Component({
  selector: 'ceiba-data-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
    LucideSearch,
    LucideX,
  ],
  templateUrl: './data-filters.component.html',
  styleUrl: './data-filters.component.scss',
})
export class DataFiltersComponent {
  readonly searchLabel = input('Buscar');
  readonly selects = input<readonly SelectFilter[]>([]);
  readonly value = input<DataFilterValue>({ search: '', selections: {} });

  readonly valueChange = output<DataFilterValue>();

  protected readonly searchControl = new FormControl('', { nonNullable: true });

  constructor() {
    effect(() => {
      const search = this.value().search;
      if (this.searchControl.value !== search) {
        this.searchControl.setValue(search, { emitEvent: false });
      }
    });

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((search) => this.valueChange.emit({ ...this.value(), search: search.trim() }));
  }

  protected selectionOf(key: string): string {
    return this.value().selections[key] ?? '';
  }

  protected onSelect(key: string, selected: string): void {
    const current = this.value();
    this.valueChange.emit({ ...current, selections: { ...current.selections, [key]: selected } });
  }

  protected hasFilters(): boolean {
    const current = this.value();
    return current.search !== '' || Object.values(current.selections).some((v) => v !== '');
  }

  protected clear(): void {
    this.searchControl.setValue('', { emitEvent: false });
    this.valueChange.emit({ search: '', selections: {} });
  }
}
