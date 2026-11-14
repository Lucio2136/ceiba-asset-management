import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_PATH } from '../../../core/configuration/api.providers';
import { Responsible } from '../../../shared/models/person.models';

/**
 * Facade de responsables (`/api/v1/responsibles`).
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere.
 */
@Injectable({ providedIn: 'root' })
export class PeopleApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/responsibles`;

  list(search?: string): Observable<Responsible[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<Responsible[]>(this.baseUrl, { params });
  }
}
