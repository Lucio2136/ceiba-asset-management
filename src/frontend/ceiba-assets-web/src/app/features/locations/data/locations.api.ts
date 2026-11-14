import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_PATH } from '../../../core/configuration/api.providers';
import { Location, LocationListQuery } from '../../../shared/models/location.models';

/**
 * Facade de ubicaciones (`/api/v1/locations`).
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere.
 */
@Injectable({ providedIn: 'root' })
export class LocationsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/locations`;

  list(query: LocationListQuery = {}): Observable<Location[]> {
    let params = new HttpParams();
    if (query.buildingId) {
      params = params.set('buildingId', query.buildingId);
    }
    if (query.floor !== undefined) {
      params = params.set('floor', query.floor);
    }
    return this.http.get<Location[]>(this.baseUrl, { params });
  }
}
