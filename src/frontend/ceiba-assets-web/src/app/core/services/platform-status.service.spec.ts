import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { provideCeibaApi } from '../configuration/api.providers';
import { PlatformStatusService } from './platform-status.service';

describe('PlatformStatusService', () => {
  it('consulta la información del sistema bajo la ruta base de la API', async () => {
    TestBed.configureTestingModule({
      providers: [provideCeibaApi(), provideHttpClientTesting()],
    });
    const service = TestBed.inject(PlatformStatusService);
    const httpTesting = TestBed.inject(HttpTestingController);
    const expected = { applicationName: 'CEIBA Asset Management', version: '0.1.0' };

    const result = firstValueFrom(service.getSystemInfo());
    const request = httpTesting.expectOne('/api/v1/system/info');
    request.flush(new Blob([JSON.stringify(expected)], { type: 'application/json' }));

    expect(request.request.method).toBe('GET');
    await expect(result).resolves.toEqual(expected);
    httpTesting.verify();
  });
});
