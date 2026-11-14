import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, map, Observable } from 'rxjs';
import { API_BASE_PATH } from '../../../core/configuration/api.providers';
import {
  AssetDetail,
  AssetListQuery,
  AssetSummary,
  AssignAssetRequest,
  AssignmentEvent,
  MoveAssetRequest,
  ReturnAssetRequest,
  SaveAssetRequest,
} from '../../../shared/models/asset.models';
import { PagedResult } from '../../../shared/models/common.models';

interface AssignmentDto {
  id: string;
  assetId: string;
  responsiblePersonId: string;
  responsiblePersonName: string;
  assignedAtUtc: string;
  returnedAtUtc?: string | null;
  notes?: string | null;
}

interface MovementDto {
  id: string;
  assetId: string;
  fromLocationName?: string | null;
  toLocationName: string;
  movedAtUtc: string;
  reason?: string | null;
}

/**
 * Facade de activos y asignaciones.
 * TEMPORAL: se reemplazará por el cliente NSwag cuando el backend lo regenere.
 */
@Injectable({ providedIn: 'root' })
export class AssetsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_PATH}/v1/assets`;
  private readonly assignmentsUrl = `${API_BASE_PATH}/v1/assignments`;

  list(query: AssetListQuery): Observable<PagedResult<AssetSummary>> {
    let params = new HttpParams().set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.type) {
      params = params.set('type', query.type);
    }
    if (query.status) {
      params = params.set('status', query.status);
    }
    return this.http.get<PagedResult<AssetSummary>>(this.baseUrl, { params });
  }

  get(id: string): Observable<AssetDetail> {
    return this.http.get<AssetDetail>(`${this.baseUrl}/${encodeURIComponent(id)}`);
  }

  create(request: SaveAssetRequest): Observable<AssetDetail> {
    return this.http.post<AssetDetail>(this.baseUrl, {
      assetType: request.assetType,
      name: request.name,
      manufacturer: request.manufacturer ?? null,
      model: request.model ?? null,
      serialNumber: request.serialNumber ?? null,
      notes: request.notes ?? null,
      locationId: request.locationId ?? null,
    });
  }

  update(id: string, request: SaveAssetRequest): Observable<AssetDetail> {
    return this.http.put<AssetDetail>(`${this.baseUrl}/${encodeURIComponent(id)}`, {
      assetType: request.assetType,
      name: request.name,
      manufacturer: request.manufacturer ?? null,
      model: request.model ?? null,
      serialNumber: request.serialNumber ?? null,
      notes: request.notes ?? null,
      status: request.status ?? null,
      rowVersion: request.rowVersion,
    });
  }

  history(id: string): Observable<AssignmentEvent[]> {
    const encoded = encodeURIComponent(id);
    return forkJoin({
      assignments: this.http.get<AssignmentDto[]>(`${this.baseUrl}/${encoded}/assignments`),
      movements: this.http.get<MovementDto[]>(`${this.baseUrl}/${encoded}/movements`),
    }).pipe(
      map(({ assignments, movements }) => {
        const assignedEvents: AssignmentEvent[] = assignments.flatMap((item) => {
          const events: AssignmentEvent[] = [
            {
              id: `${item.id}-a`,
              kind: 'Assigned',
              occurredAtUtc: item.assignedAtUtc,
              responsibleName: item.responsiblePersonName,
              notes: item.notes,
            },
          ];
          if (item.returnedAtUtc) {
            events.push({
              id: `${item.id}-r`,
              kind: 'Returned',
              occurredAtUtc: item.returnedAtUtc,
              responsibleName: item.responsiblePersonName,
              notes: item.notes,
            });
          }
          return events;
        });

        const movedEvents: AssignmentEvent[] = movements.map((item) => ({
          id: item.id,
          kind: 'Moved',
          occurredAtUtc: item.movedAtUtc,
          locationName: item.toLocationName,
          notes: item.reason,
        }));

        return [...assignedEvents, ...movedEvents].sort((a, b) =>
          a.occurredAtUtc < b.occurredAtUtc ? 1 : -1,
        );
      }),
    );
  }

  assign(request: AssignAssetRequest): Observable<unknown> {
    return this.http.post(this.assignmentsUrl, request);
  }

  return(request: ReturnAssetRequest): Observable<unknown> {
    return this.http.post(`${this.assignmentsUrl}/return`, request);
  }

  move(request: MoveAssetRequest): Observable<unknown> {
    return this.http.post(`${this.assignmentsUrl}/move`, request);
  }
}
