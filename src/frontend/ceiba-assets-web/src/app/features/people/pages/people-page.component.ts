import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import {
  DataFilterValue,
  DataFiltersComponent,
} from '../../../shared/components/data-filters/data-filters.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { PeopleApi } from '../data/people.api';

@Component({
  selector: 'ceiba-people-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DataFiltersComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './people-page.component.html',
  styleUrl: './people-page.component.scss',
})
export class PeoplePageComponent {
  private readonly api = inject(PeopleApi);

  protected readonly columns = ['employeeNumber', 'displayName', 'email', 'department'];
  protected readonly filters = signal<DataFilterValue>({ search: '', selections: {} });

  protected readonly people = rxResource({
    params: () => ({ search: this.filters().search }),
    stream: ({ params }) =>
      this.api.list({ search: params.search || undefined, page: 1, pageSize: 100 }),
  });

  protected readonly items = computed(() =>
    this.people.hasValue() ? this.people.value().items : [],
  );
}
