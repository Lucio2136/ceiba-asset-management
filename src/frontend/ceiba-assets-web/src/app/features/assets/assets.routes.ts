import { Routes } from '@angular/router';

const TITLE_SUFFIX = 'CEIBA Asset Management';

/** Rutas lazy de la funcionalidad de activos (`/assets/*`). El orden importa: `new` antes de `:id`. */
export const ASSETS_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: `Inventario | ${TITLE_SUFFIX}`,
    loadComponent: () =>
      import('./pages/asset-list-page.component').then((m) => m.AssetListPageComponent),
  },
  {
    path: 'new',
    title: `Nuevo activo | ${TITLE_SUFFIX}`,
    loadComponent: () => import('./pages/asset-form.component').then((m) => m.AssetFormComponent),
  },
  {
    path: ':id/edit',
    title: `Editar activo | ${TITLE_SUFFIX}`,
    loadComponent: () => import('./pages/asset-form.component').then((m) => m.AssetFormComponent),
  },
  {
    path: ':id',
    title: `Detalle del activo | ${TITLE_SUFFIX}`,
    loadComponent: () =>
      import('./pages/asset-detail.component').then((m) => m.AssetDetailComponent),
  },
];
