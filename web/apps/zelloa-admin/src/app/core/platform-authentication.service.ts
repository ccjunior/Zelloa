import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CurrentUser, PlatformApiService } from './platform-api.service';

@Injectable({ providedIn: 'root' })
export class PlatformAuthenticationService {
  private readonly api = inject(PlatformApiService);
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
    if (current.tenantId !== null || current.roles.length !== 1 || current.roles[0] !== 'PlatformAdmin') {
      await this.clearRejectedSession();
      throw new Error('Esta conta não tem acesso à administração da plataforma.');
    }
  }

  async clearRejectedSession(): Promise<void> {
    try {
      await this.api.post<void>('/api/auth/logout');
    } catch {
      // Clear local state even if the session has already expired.
    } finally {
      this.user.set(null);
    }
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
