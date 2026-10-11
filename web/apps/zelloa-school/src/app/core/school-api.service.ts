import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SchoolApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  get<T>(path: string): Promise<T> {
    return firstValueFrom(
      this.http.get<T>(`${this.baseUrl}${path}`, { withCredentials: true }),
    );
  }

  post<T>(path: string, body: unknown = {}): Promise<T> {
    return this.mutate<T>('POST', path, body);
  }

  put<T>(path: string, body: unknown): Promise<T> {
    return this.mutate<T>('PUT', path, body);
  }

  patch<T>(path: string, body: unknown): Promise<T> {
    return this.mutate<T>('PATCH', path, body);
  }

  delete<T>(path: string): Promise<T> {
    return this.mutate<T>('DELETE', path);
  }

  private async mutate<T>(method: string, path: string, body?: unknown): Promise<T> {
    const csrf = await firstValueFrom(
      this.http.get<{ requestToken: string }>(`${this.baseUrl}/api/auth/csrf`, {
        withCredentials: true,
      }),
    );

    return firstValueFrom(
      this.http.request<T>(method, `${this.baseUrl}${path}`, {
        body,
        headers: new HttpHeaders({ 'X-XSRF-TOKEN': csrf.requestToken }),
        withCredentials: true,
      }),
    );
  }
}

export interface CurrentUser {
  userId: string;
  displayName: string;
  tenantId: string | null;
  roles: string[];
}

export interface SchoolInfo {
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

export interface Classroom {
  id: string;
  name: string;
  level: string;
  shift: string;
  academicYear: number;
  isActive: boolean;
}

export interface Student {
  id: string;
  name: string;
  classroomId: string;
  classroomName: string;
  isActive: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface GuardianInvitation {
  userId: string;
  activationUrl: string;
  expiresAt: string;
}

export interface CatalogCategory {
  id: string;
  name: string;
  isActive: boolean;
}

export interface CatalogProduct {
  id: string;
  name: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
  price: number;
  imageUrl: string | null;
  isActive: boolean;
  isAvailable: boolean;
}

export interface PagedCatalogResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface SchoolOrder {
  orderId: string;
  studentId: string;
  studentName: string;
  classroomName: string;
  shift: string;
  operationalDate: string;
  status: string;
  createdAt: string;
  items: { productId: string; productName: string; unitPrice: number; quantity: number; subtotal: number }[];
  total: number;
}
