import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/authentication/auth.guards';
import { ShellComponent } from './core/layout/shell/shell.component';

const TITLE_SUFFIX = 'CEIBA Asset Management';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    title: `Iniciar sesión | ${TITLE_SUFFIX}`,
    loadComponent: () =>
      import('./features/auth/login/login-page.component').then((m) => m.LoginPageComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'assets' },
      {
        path: 'buildings',
        title: `Edificios | ${TITLE_SUFFIX}`,
        loadComponent: () =>
          import('./features/buildings/pages/buildings-page.component').then(
            (m) => m.BuildingsPageComponent,
          ),
      },
      {
        path: 'locations',
        title: `Ubicaciones | ${TITLE_SUFFIX}`,
        loadComponent: () =>
          import('./features/locations/pages/locations-page.component').then(
            (m) => m.LocationsPageComponent,
          ),
      },
      {
        path: 'assets',
        loadChildren: () => import('./features/assets/assets.routes').then((m) => m.ASSETS_ROUTES),
      },
      {
        path: 'people',
        title: `Responsables | ${TITLE_SUFFIX}`,
        loadComponent: () =>
          import('./features/people/pages/people-page.component').then(
            (m) => m.PeoplePageComponent,
          ),
      },
      {
        path: 'status',
        title: `Estado de la API | ${TITLE_SUFFIX}`,
        loadComponent: () =>
          import('./core/layout/home/home-page.component').then((m) => m.HomePageComponent),
      },
      {
        path: '**',
        title: `Página no encontrada | ${TITLE_SUFFIX}`,
        loadComponent: () =>
          import('./core/layout/not-found/not-found-page.component').then(
            (m) => m.NotFoundPageComponent,
          ),
      },
    ],
  },
];
