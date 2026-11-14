import { Routes } from '@angular/router';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'Inicio | CEIBA Asset Management',
        loadComponent: () =>
          import('./core/layout/home/home-page.component').then((m) => m.HomePageComponent),
      },
      {
        path: '**',
        title: 'Página no encontrada | CEIBA Asset Management',
        loadComponent: () =>
          import('./core/layout/not-found/not-found-page.component').then(
            (m) => m.NotFoundPageComponent,
          ),
      },
    ],
  },
];
