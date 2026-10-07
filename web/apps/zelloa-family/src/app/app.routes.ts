import { Routes } from '@angular/router';
import { guardianGuard } from './core/guardian.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login-page').then(module => module.FamilyLoginPage) },
  { path: 'activate-invitation', loadComponent: () => import('./auth/activate-invitation-page').then(module => module.FamilyActivateInvitationPage) },
  { path: '', pathMatch: 'full', canActivate: [guardianGuard], loadComponent: () => import('./home/family-home-page').then(module => module.FamilyHomePage) },
  { path: '**', redirectTo: '/login' },
];
