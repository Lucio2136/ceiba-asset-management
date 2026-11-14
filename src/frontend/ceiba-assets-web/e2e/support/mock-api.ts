import { Page, Route } from '@playwright/test';

export const testUser = {
  id: 'u-1',
  userName: 'ana',
  displayName: 'Ana Pérez',
  email: 'ana@ceiba.test',
  roles: ['Admin'],
};

export const validPassword = 'Correcta-123';

export const assets = [
  {
    id: 'a-1',
    name: 'Laptop Dell Latitude',
    type: 'Laptop',
    status: 'Assigned',
    serialNumber: 'DL-0001',
    locationId: 'l-1',
    locationName: 'Torre A · Piso 3',
    assignedToId: 'p-1',
    assignedToName: 'Luis Gómez',
  },
  {
    id: 'a-2',
    name: 'Monitor LG 27',
    type: 'Monitor',
    status: 'Available',
    serialNumber: 'LG-0042',
    locationId: 'l-2',
    locationName: 'Torre B · Piso 1',
    assignedToId: null,
    assignedToName: null,
  },
  {
    id: 'a-3',
    name: 'Servidor Ecommerce',
    type: 'Server',
    status: 'InMaintenance',
    serialNumber: null,
    locationId: 'l-3',
    locationName: 'Ecommerce · Piso 1',
    assignedToId: null,
    assignedToName: null,
  },
];

const problem = (status: number, title: string) => ({
  status,
  contentType: 'application/problem+json',
  body: JSON.stringify({ status, title }),
});

export interface MockApi {
  /** URLs de la API solicitadas y no simuladas; deben quedar vacías al final de la prueba. */
  unexpectedCalls: string[];
  /** Consultas recibidas por `GET /api/v1/assets`. */
  assetQueries: URLSearchParams[];
}

/**
 * Simula la API: ninguna prueba E2E depende de un backend real. Toda llamada a `/api/**` que no
 * se simule aquí se aborta y se registra en `unexpectedCalls`.
 */
export async function mockApi(
  page: Page,
  options: { authenticated?: boolean } = {},
): Promise<MockApi> {
  let authenticated = options.authenticated ?? false;
  const state: MockApi = { unexpectedCalls: [], assetQueries: [] };

  await page.route('**/api/**', async (route) => {
    state.unexpectedCalls.push(route.request().url());
    await route.abort();
  });

  await page.route('**/api/v1/auth/me', (route: Route) =>
    authenticated ? route.fulfill({ json: testUser }) : route.fulfill(problem(401, 'Unauthorized')),
  );

  await page.route('**/api/v1/auth/login', (route) => {
    const body = route.request().postDataJSON() as { password?: string };
    if (body.password === validPassword) {
      authenticated = true;
      return route.fulfill({ json: testUser });
    }
    return route.fulfill(problem(401, 'Unauthorized'));
  });

  await page.route('**/api/v1/auth/logout', (route) => {
    authenticated = false;
    return route.fulfill({ status: 204 });
  });

  await page.route(/\/api\/v1\/assets(\?.*)?$/, (route) => {
    const query = new URL(route.request().url()).searchParams;
    state.assetQueries.push(query);
    const search = (query.get('search') ?? '').toLowerCase();
    const items = assets.filter((asset) => asset.name.toLowerCase().includes(search));
    return route.fulfill({
      json: { items, totalCount: items.length, page: 1, pageSize: Number(query.get('pageSize')) },
    });
  });

  await page.route('**/api/v1/system/info', (route) =>
    route.fulfill({ json: { applicationName: 'CEIBA Asset Management', version: '0.1.0' } }),
  );

  return state;
}
