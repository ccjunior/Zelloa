import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FamilyApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  get<T>(path: string): Promise<T> {
    return firstValueFrom(this.http.get<T>(`${this.baseUrl}${path}`, { withCredentials: true }));
  }

  post<T>(path: string, body: unknown = {}): Promise<T> {
    return this.mutate<T>(path, body);
  }

  private async mutate<T>(path: string, body: unknown): Promise<T> {
    const csrf = await firstValueFrom(
      this.http.get<{ requestToken: string }>(`${this.baseUrl}/api/auth/csrf`, { withCredentials: true }),
    );

    return firstValueFrom(this.http.post<T>(`${this.baseUrl}${path}`, body, {
      headers: new HttpHeaders({ 'X-XSRF-TOKEN': csrf.requestToken }),
      withCredentials: true,
    }));
  }
}

export interface FamilyCurrentUser {
  userId: string;
  displayName: string;
  tenantId: string | null;
  roles: string[];
}

export interface LinkedStudent {
  id: string;
  name: string;
  classroomId: string;
  classroomName: string;
  isActive: boolean;
}

export interface AvailableCatalogProduct {
  id: string;
  name: string;
  description: string | null;
  price: number;
  imageUrl: string | null;
}

export interface AvailableCatalogCategory {
  id: string;
  name: string;
  products: AvailableCatalogProduct[];
}

export interface AvailableCatalogResponse {
  categories: AvailableCatalogCategory[];
}

export interface CreateOrderRequest {
  studentId: string;
  items: { productId: string; quantity: number }[];
}

export interface OrderResponse {
  orderId: string;
  studentId: string;
  classroomName: string;
  shift: string;
  operationalDate: string;
  status: string;
  createdAt: string;
  items: { productId: string; productName: string; unitPrice: number; quantity: number; subtotal: number }[];
  total: number;
}
