import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CurrentUser, SchoolApiService } from './school-api.service';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
  private readonly api = inject(SchoolApiService);
  private readonly router = inject(Router);

  readonly user = signal<CurrentUser | null>(null);

  async loadCurrentUser(): Promise<CurrentUser> {
    const current = await this.api.get<CurrentUser>('/api/me');
    this.user.set(current);
    return current;
  }

  async login(email: string, password: string): Promise<void> {
    await this.api.post<void>('/api/auth/login', { email, password });
    const current = await this.loadCurrentUser();
    if (!current.roles.includes('SchoolAdmin')) {
      try {
        await this.api.post<void>('/api/auth/logout');
      } finally {
        this.user.set(null);
      }
      throw new Error('Sua conta não tem acesso ao painel escolar. Entre com uma conta de administrador da escola.');
    }
  }

  async activate(email: string, token: string, password: string): Promise<void> {
    await this.api.post<void>('/api/auth/activate', { email, token, password });
  }

  async logout(): Promise<void> {
    try {
      await this.api.post<void>('/api/auth/logout');
    } finally {
      this.user.set(null);
      await this.router.navigateByUrl('/login');
    }
  }
}
