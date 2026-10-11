import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FamilyApiService, LinkedStudent, OrderResponse } from '../core/family-api.service';
import { FamilyAuthenticationService } from '../core/family-authentication.service';
import { FamilyCartService } from '../core/family-cart.service';

@Component({
  selector: 'app-family-cart-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './family-cart-page.html',
  styleUrl: './family-cart-page.scss',
})
export class FamilyCartPage implements OnInit {
  private readonly api = inject(FamilyApiService);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);
  readonly cart = inject(FamilyCartService);

  readonly students = signal<LinkedStudent[]>([]);
  readonly selectedStudentId = signal('');
  readonly loadingStudents = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly createdOrder = signal<OrderResponse | null>(null);
  readonly selectedStudent = computed(() => this.students().find(student => student.id === this.selectedStudentId()) ?? null);

  ngOnInit(): void {
    void this.loadStudents();
  }

  async loadStudents(): Promise<void> {
    this.loadingStudents.set(true);
    this.errorMessage.set('');
    try {
      const linked = await this.api.get<LinkedStudent[]>('/api/me/students');
      const active = linked.filter(student => student.isActive);
      this.students.set(active);
      if (!active.some(student => student.id === this.selectedStudentId())) {
        this.selectedStudentId.set(active.length === 1 ? active[0].id : '');
      }
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.user.set(null);
        this.cart.clear();
        await this.router.navigateByUrl('/login');
        return;
      }
      this.errorMessage.set('Não foi possível carregar os alunos vinculados. Tente novamente.');
    } finally {
      this.loadingStudents.set(false);
    }
  }

  selectStudent(event: Event): void {
    this.selectedStudentId.set((event.target as HTMLSelectElement).value);
  }

  changeQuantity(productId: string, event: Event): void {
    const quantity = Number((event.target as HTMLInputElement).value);
    this.cart.setQuantity(productId, quantity);
  }

  async submitOrder(): Promise<void> {
    if (!this.selectedStudentId() || this.cart.items().length === 0 || this.submitting()) return;

    this.submitting.set(true);
    this.errorMessage.set('');
    try {
      const order = await this.api.post<OrderResponse>('/api/orders', {
        studentId: this.selectedStudentId(),
        items: this.cart.items().map(item => ({ productId: item.id, quantity: item.quantity })),
      });
      this.createdOrder.set(order);
      this.cart.clear();
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.user.set(null);
        this.cart.clear();
        await this.router.navigateByUrl('/login');
        return;
      }
      const serverMessage = error instanceof HttpErrorResponse && typeof error.error?.message === 'string'
        ? error.error.message
        : '';
      this.errorMessage.set(serverMessage || 'Não foi possível enviar o pedido agora. Confira os itens e tente novamente.');
    } finally {
      this.submitting.set(false);
    }
  }
}
