import { Routes } from '@angular/router';
import { schoolAdminGuard } from './core/school-admin.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./auth/login-page').then(module => module.LoginPage) },
  { path: 'activate-invitation', loadComponent: () => import('./auth/activate-invitation-page').then(module => module.ActivateInvitationPage) },
  {
    path: '', canActivate: [schoolAdminGuard],
    loadComponent: () => import('./school/school-shell').then(module => module.SchoolShell),
    children: [
      { path: '', pathMatch: 'full', loadComponent: () => import('./school/dashboard-page').then(module => module.DashboardPage) },
      { path: 'classrooms', loadComponent: () => import('./school/classrooms-page').then(module => module.ClassroomsPage) },
      { path: 'students', loadComponent: () => import('./school/students-page').then(module => module.StudentsPage) },
      { path: 'guardians', loadComponent: () => import('./school/guardians-page').then(module => module.GuardiansPage) },
    ],
  },
  { path: '**', redirectTo: '' },
];
