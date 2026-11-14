import { defineConfig, devices } from '@playwright/test';

const isCi = Boolean(process.env['CI']);

// E2E_BASE_URL permite ejecutar la misma suite contra un entorno desplegado (por ejemplo, IIS).
const externalBaseUrl = process.env['E2E_BASE_URL'];
const baseURL = externalBaseUrl ?? 'http://localhost:4200';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: isCi,
  retries: isCi ? 2 : 0,
  reporter: isCi ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'edge',
      use: { ...devices['Desktop Edge'], channel: 'msedge' },
    },
  ],
  // Sin E2E_BASE_URL se sirve la compilación de producción: los mismos bundles que publicará IIS.
  webServer: externalBaseUrl
    ? undefined
    : {
        command: 'npm run start -- --configuration production --port 4200',
        url: baseURL,
        reuseExistingServer: !isCi,
        timeout: 120_000,
      },
});
