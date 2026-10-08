import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { SchoolApiService, SchoolOrder } from '../core/school-api.service';

@Component({
  selector: 'app-orders-page',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
})
export class OrdersPage {
  private readonly api = inject(SchoolApiService);

  readonly orders = signal<SchoolOrder[]>([]);
  readonly loading = signal(false);
  readonly errorMessage = signal('');
  readonly searchText = signal('');
  readonly statusFilter = signal('');
  readonly filteredOrders = computed(() => {
    const query = this.searchText().trim().toLocaleLowerCase('pt-BR');
    const status = this.statusFilter();
    return this.orders().filter(order => {
      const matchesStatus = !status || order.status === status;
      const matchesQuery = !query || `${order.studentName} ${order.classroomName} ${order.shift}`
        .toLocaleLowerCase('pt-BR').includes(query);
      return matchesStatus && matchesQuery;
    });
  });
  readonly awaitingPaymentCount = computed(() =>
    this.orders().filter(order => order.status === 'AwaitingPayment').length,
  );

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.errorMessage.set('');
    try {
      this.orders.set(await this.api.get<SchoolOrder[]>('/api/school/orders'));
    } catch {
      this.errorMessage.set('Não foi possível carregar os pedidos. Tente novamente.');
    } finally {
      this.loading.set(false);
    }
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      AwaitingPayment: 'Aguardando pagamento',
      Paid: 'Pago',
      Preparing: 'Em preparo',
      Ready: 'Pronto',
      Delivered: 'Entregue',
      Cancelled: 'Cancelado',
      PaymentExpired: 'Pagamento expirado',
    };
    return labels[status] ?? status;
  }

  statusClass(status: string): string {
    return status === 'AwaitingPayment' ? 'pending'
      : status === 'Cancelled' || status === 'PaymentExpired' ? 'closed'
        : 'active';
  }
}
