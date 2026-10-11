import { Routes } from '@angular/router';
import { guardianGuard } from './core/guardian.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login-page').then(module => module.FamilyLoginPage) },
  { path: 'activate-invitation', loadComponent: () => import('./auth/activate-invitation-page').then(module => module.FamilyActivateInvitationPage) },
  { path: 'catalog', canActivate: [guardianGuard], loadComponent: () => import('./catalog/catalog-page').then(module => module.FamilyCatalogPage) },
  { path: 'cart', canActivate: [guardianGuard], loadComponent: () => import('./orders/family-cart-page').then(module => module.FamilyCartPage) },
  { path: 'orders', canActivate: [guardianGuard], loadComponent: () => import('./orders/family-orders-page').then(module => module.FamilyOrdersPage) },
  { path: '', pathMatch: 'full', canActivate: [guardianGuard], loadComponent: () => import('./home/family-home-page').then(module => module.FamilyHomePage) },
  { path: '**', redirectTo: '/login' },
];
