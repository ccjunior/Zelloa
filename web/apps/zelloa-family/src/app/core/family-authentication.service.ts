import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FamilyApiService, FamilyCurrentUser } from './family-api.service';
import { FamilyCartService } from './family-cart.service';

@Injectable({ providedIn: 'root' })
export class FamilyAuthenticationService {
  private readonly api = inject(FamilyApiService);
  private readonly router = inject(Router);
  private readonly cart = inject(FamilyCartService);

  readonly user = signal<FamilyCurrentUser | null>(null);

  async loadCurrentUser(): Promise<FamilyCurrentUser> {
    const current = await this.api.get<FamilyCurrentUser>('/api/me');
    this.user.set(current);
    return current;
  }

  async login(email: string, password: string): Promise<void> {
    await this.api.post<void>('/api/auth/login', { email, password });
    const current = await this.loadCurrentUser();
    if (!current.roles.includes('Guardian')) {
      try {
        await this.api.post<void>('/api/auth/logout');
      } finally {
        this.user.set(null);
        this.cart.clear();
      }
      throw new Error('Esta conta não tem acesso ao portal da família. Entre com uma conta de responsável.');
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
      this.cart.clear();
      await this.router.navigateByUrl('/login');
    }
  }
}
