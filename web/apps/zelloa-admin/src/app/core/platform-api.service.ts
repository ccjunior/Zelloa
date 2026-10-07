import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PlatformApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  get<T>(path: string): Promise<T> {
    return firstValueFrom(this.http.get<T>(`${this.baseUrl}${path}`, { withCredentials: true }));
  }

  post<T>(path: string, body: unknown = {}): Promise<T> {
    return this.mutate<T>('POST', path, body);
  }

  private async mutate<T>(method: string, path: string, body: unknown): Promise<T> {
    const csrf = await firstValueFrom(
      this.http.get<{ requestToken: string }>(`${this.baseUrl}/api/auth/csrf`, { withCredentials: true }),
    );

    return firstValueFrom(this.http.request<T>(method, `${this.baseUrl}${path}`, {
      body,
      headers: new HttpHeaders({ 'X-XSRF-TOKEN': csrf.requestToken }),
      withCredentials: true,
    }));
  }
}

export interface CurrentUser {
  userId: string;
  displayName: string;
  tenantId: string | null;
  roles: string[];
}

export interface SchoolResponse {
  id: string;
  tenantId: string;
  name: string;
  tradeName: string;
  identifier: string | null;
  contactEmail: string | null;
  contactPhone: string | null;
  timeZone: string;
  isActive: boolean;
}

export interface InvitationResponse {
  userId: string;
  activationUrl: string;
  expiresAt: string;
}
