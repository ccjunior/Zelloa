import { Routes } from '@angular/router';
import { platformAdminGuard } from './core/platform-admin.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login-page').then(module => module.LoginPage) },
  {
    path: '',
    canActivate: [platformAdminGuard],
    loadComponent: () => import('./admin/admin-shell').then(module => module.AdminShell),
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./institutions/create-institution-page').then(module => module.CreateInstitutionPage) },
      { path: 'convites/renovar', loadComponent: () => import('./institutions/renew-school-admin-invitation-page').then(module => module.RenewSchoolAdminInvitationPage) },
    ],
  },
  { path: '**', redirectTo: '' },
];
