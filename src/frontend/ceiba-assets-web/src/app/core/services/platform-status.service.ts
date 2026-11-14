import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SystemClient, SystemInfoResponse } from '../http/generated/ceiba-api.client';

/**
 * Consulta la identificación y disponibilidad de la API.
 */
@Injectable({ providedIn: 'root' })
export class PlatformStatusService {
  private readonly systemClient = inject(SystemClient);

  getSystemInfo(): Observable<SystemInfoResponse> {
    return this.systemClient.getInfo();
  }
}
