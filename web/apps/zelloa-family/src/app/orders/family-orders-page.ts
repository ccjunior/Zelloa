import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FamilyApiService, LinkedStudent, OrderResponse } from '../core/family-api.service';
import { FamilyAuthenticationService } from '../core/family-authentication.service';

@Component({
  selector: 'app-family-orders-page',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './family-orders-page.html',
  styleUrl: './family-orders-page.scss',
})
export class FamilyOrdersPage implements OnInit {
  private readonly api = inject(FamilyApiService);
  private readonly auth = inject(FamilyAuthenticationService);
  private readonly router = inject(Router);

  readonly orders = signal<OrderResponse[]>([]);
  readonly studentNames = signal<Record<string, string>>({});
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit(): void {
    void this.loadOrders();
  }

  async loadOrders(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');

    try {
      const [orders, students] = await Promise.all([
        this.api.get<OrderResponse[]>('/api/me/orders'),
        this.api.get<LinkedStudent[]>('/api/me/students'),
      ]);
      this.orders.set(orders);
      this.studentNames.set(Object.fromEntries(students.map(student => [student.id, student.name])));
    } catch (error) {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.user.set(null);
        await this.router.navigateByUrl('/login');
        return;
      }
      this.errorMessage.set('Não foi possível carregar seus pedidos. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  studentName(order: OrderResponse): string {
    return this.studentNames()[order.studentId] ?? 'Aluno vinculado';
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      AwaitingPayment: 'Aguardando pagamento',
      Paid: 'Pago',
      Preparing: 'Em preparo',
      Ready: 'Pronto para retirada',
      Delivered: 'Entregue',
      Cancelled: 'Cancelado',
      PaymentExpired: 'Pagamento expirado',
    };
    return labels[status] ?? 'Status atualizado';
  }

  statusClass(status: string): string {
    return `status-${status.replace(/[^a-zA-Z]/g, '').toLowerCase()}`;
  }

  operationalDate(value: string): string {
    const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(value);
    return match ? `${match[3]}/${match[2]}/${match[1]}` : value;
  }

  createdAt(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? value
      : new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(date);
  }
}
