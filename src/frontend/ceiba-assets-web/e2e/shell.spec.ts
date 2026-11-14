import { expect, test } from '@playwright/test';
import { assets, MockApi, mockApi, testUser, validPassword } from './support/mock-api';

let api: MockApi;

test.afterEach(() => {
  expect(api.unexpectedCalls).toEqual([]);
});

test.describe('Autenticación', () => {
  test('redirige a /login cuando no hay sesión y conserva el destino', async ({ page }) => {
    api = await mockApi(page);

    await page.goto('/assets');

    await expect(page).toHaveURL(/\/login\?returnUrl=%2Fassets$/);
    await expect(page).toHaveTitle('Iniciar sesión | CEIBA Asset Management');
    await expect(page.getByRole('heading', { level: 1, name: 'Iniciar sesión' })).toBeVisible();
    // Sin sesión no se muestra la navegación.
    await expect(page.getByRole('navigation', { name: 'Navegación principal' })).toHaveCount(0);
  });

  test('muestra un error con credenciales incorrectas', async ({ page }) => {
    api = await mockApi(page);
    await page.goto('/login');

    await page.getByLabel('Usuario').fill(testUser.userName);
    await page.getByLabel('Contraseña').fill('incorrecta');
    await page.getByRole('button', { name: 'Entrar' }).click();

    await expect(page.getByRole('alert')).toContainText('Usuario o contraseña incorrectos.');
    await expect(page).toHaveURL(/\/login/);
  });

  test('inicia sesión, ve el inventario y cierra sesión', async ({ page }) => {
    api = await mockApi(page);
    await page.goto('/login');

    await page.getByLabel('Usuario').fill(testUser.userName);
    await page.getByLabel('Contraseña').fill(validPassword);
    await page.getByRole('button', { name: 'Entrar' }).click();

    await expect(page).toHaveURL(/\/assets$/);
    await expect(
      page.getByRole('heading', { level: 1, name: 'Inventario de activos' }),
    ).toBeVisible();
    await expect(page.getByTestId('current-user')).toContainText(testUser.displayName);

    // AG Grid pinta la página de activos devuelta por la API.
    const grid = page.getByRole('region', { name: 'Inventario de activos' });
    for (const asset of assets) {
      await expect(grid.getByRole('link', { name: asset.name, exact: true })).toBeVisible();
    }
    await expect(grid.getByText('Asignado', { exact: true })).toBeVisible();
    await expect(grid.getByText('Luis Gómez')).toBeVisible();

    await page.getByRole('button', { name: 'Cerrar sesión' }).click();
    await expect(page).toHaveURL(/\/login/);
    await expect(page.getByTestId('current-user')).toHaveCount(0);
  });

  test('un usuario con sesión no ve /login y va al inventario', async ({ page }) => {
    api = await mockApi(page, { authenticated: true });

    await page.goto('/login');

    await expect(page).toHaveURL(/\/assets$/);
  });
});

test.describe('Inventario', () => {
  test('la búsqueda se envía al servidor', async ({ page }) => {
    api = await mockApi(page, { authenticated: true });
    await page.goto('/assets');
    const grid = page.getByRole('region', { name: 'Inventario de activos' });
    await expect(grid.getByRole('link', { name: 'Monitor LG 27' })).toBeVisible();

    await page.getByLabel('Buscar por nombre o serie').fill('monitor');

    await expect(grid.getByRole('link', { name: 'Laptop Dell Latitude' })).toHaveCount(0);
    await expect(grid.getByRole('link', { name: 'Monitor LG 27' })).toBeVisible();
    expect(api.assetQueries.at(-1)?.get('search')).toBe('monitor');
    expect(api.assetQueries.at(-1)?.get('page')).toBe('1');
  });

  test('muestra un error recuperable cuando la API falla', async ({ page }) => {
    api = await mockApi(page, { authenticated: true });
    await page.route(/\/api\/v1\/assets(\?.*)?$/, (route) =>
      route.fulfill({
        status: 503,
        contentType: 'application/problem+json',
        body: '{"status":503}',
      }),
    );

    await page.goto('/assets');

    await expect(page.getByRole('alert')).toContainText('Error del servidor');
    await expect(page.getByRole('button', { name: 'Reintentar' })).toBeVisible();
  });
});

test.describe('Shell de la aplicación', () => {
  test('muestra el estado de la API', async ({ page }) => {
    api = await mockApi(page, { authenticated: true });

    await page.goto('/status');

    await expect(page).toHaveTitle('Estado de la API | CEIBA Asset Management');
    await expect(page.getByText('API operativa')).toBeVisible();
    await expect(page.getByText('0.1.0')).toBeVisible();
  });

  test('muestra la página no encontrada para rutas desconocidas', async ({ page }) => {
    api = await mockApi(page, { authenticated: true });

    await page.goto('/ruta-inexistente');

    await expect(page).toHaveTitle('Página no encontrada | CEIBA Asset Management');
    await expect(page.getByRole('heading', { name: 'Página no encontrada' })).toBeVisible();
    await expect(page.getByRole('link', { name: 'Volver al inicio' })).toBeVisible();
  });
});
