import { registerLocaleData } from '@angular/common';
import localeEsMx from '@angular/common/locales/es-MX';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { provideCeibaApi } from './core/configuration/api.providers';

registerLocaleData(localeEsMx);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // Los parámetros de ruta y de consulta llegan como `input()` a los componentes de página.
    provideRouter(routes, withComponentInputBinding()),
    provideCeibaApi(),
    { provide: LOCALE_ID, useValue: 'es-MX' },
  ],
};
