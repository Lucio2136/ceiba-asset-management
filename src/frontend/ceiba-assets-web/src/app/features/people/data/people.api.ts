import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_PATH } from '../../../core/configuration/api.providers';
import { PagedResult } from '../../../shared/models/common.models';
import { Responsible, ResponsibleListQuery } from '../../../shared/models/person.models';

/**
 * Facade de responsables (`/api/v1/responsible-people`).
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere.
 */
@Injectable({ providedIn: 'root' })
export class PeopleApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/responsible-people`;

  list(query: ResponsibleListQuery = {}): Observable<PagedResult<Responsible>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 25);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.includeInactive) {
      params = params.set('includeInactive', 'true');
    }
    return this.http.get<PagedResult<Responsible>>(this.baseUrl, { params });
  }
}
