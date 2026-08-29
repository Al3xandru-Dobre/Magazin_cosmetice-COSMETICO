import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { Order, ORDER_STATUSES } from '../../../shared/models/order.models';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-admin-orders',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './admin-orders.component.html',
})
export class AdminOrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);

  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly statuses = ORDER_STATUSES;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.orderService.getAll().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(extractApiError(err));
        this.loading.set(false);
      },
    });
  }

  onStatusChange(order: Order, event: Event): void {
    const status = (event.target as HTMLSelectElement).value;
    this.error.set(null);
    this.successMessage.set(null);

    this.orderService.updateStatus(order.id, status).subscribe({
      next: (updated) => {
        this.successMessage.set(`Comanda #${updated.id}: status schimbat in "${updated.status}".`);
        this.load();
      },
      error: (err) => this.error.set(extractApiError(err)),
    });
  }

  statusClass(status: string): string {
    switch (status) {
      case 'Paid': return 'text-bg-primary';
      case 'Shipped': return 'text-bg-info';
      case 'Delivered': return 'text-bg-success';
      case 'Cancelled': return 'text-bg-danger';
      default: return 'text-bg-secondary';
    }
  }
}
