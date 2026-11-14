import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_PATH } from '../../../core/configuration/api.providers';
import { Building, Floor } from '../../../shared/models/location.models';

/**
 * Facade de edificios (`/api/v1/buildings`).
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere.
 */
@Injectable({ providedIn: 'root' })
export class BuildingsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/buildings`;

  list(): Observable<Building[]> {
    return this.http.get<Building[]>(this.baseUrl);
  }

  floors(buildingId: string): Observable<Floor[]> {
    return this.http.get<Floor[]>(`${this.baseUrl}/${encodeURIComponent(buildingId)}/floors`);
  }
}
