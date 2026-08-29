import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../shared/models/order.models';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-my-orders',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './my-orders.component.html',
})
export class MyOrdersComponent implements OnInit {
  private readonly orderService = inject(OrderService);

  protected readonly orders = signal<Order[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.orderService.getMyOrders().subscribe({
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
