import { expect, test } from '@playwright/test';

const systemInfoRoute = '**/api/v1/system/info';

let unexpectedApiCalls: string[];

// Las pruebas no dependen de un backend real: cualquier llamada a la API que una prueba no
// simule explícitamente se aborta y hace fallar la prueba.
test.beforeEach(async ({ page }) => {
  unexpectedApiCalls = [];
  await page.route('**/api/**', async (route) => {
    unexpectedApiCalls.push(route.request().url());
    await route.abort();
  });
});

test.afterEach(() => {
  expect(unexpectedApiCalls).toEqual([]);
});

test.describe('Shell de la aplicación', () => {
  test('muestra el inicio con el estado de la API', async ({ page }) => {
    await page.route(systemInfoRoute, (route) =>
      route.fulfill({ json: { applicationName: 'CEIBA Asset Management', version: '0.1.0' } }),
    );

    await page.goto('/');

    await expect(page).toHaveTitle('Inicio | CEIBA Asset Management');
    await expect(page.getByRole('heading', { level: 1, name: 'Inicio' })).toBeVisible();
    await expect(page.getByText('API operativa')).toBeVisible();
    await expect(page.getByText('0.1.0')).toBeVisible();
  });

  test('muestra un error recuperable cuando la API no está disponible', async ({ page }) => {
    await page.route(systemInfoRoute, (route) =>
      route.fulfill({
        status: 503,
        contentType: 'application/problem+json',
        body: JSON.stringify({ status: 503, title: 'Service Unavailable' }),
      }),
    );

    await page.goto('/');

    await expect(page.getByRole('alert')).toContainText('No fue posible contactar la API');
    await expect(page.getByRole('button', { name: 'Reintentar' })).toBeVisible();
  });

  test('muestra la página no encontrada para rutas desconocidas', async ({ page }) => {
    await page.goto('/ruta-inexistente');

    await expect(page).toHaveTitle('Página no encontrada | CEIBA Asset Management');
    await expect(page.getByRole('heading', { name: 'Página no encontrada' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Volver al inicio' })).toBeVisible();
  });
});
