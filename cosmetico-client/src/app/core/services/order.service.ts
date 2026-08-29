import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Order, CreateOrderRequest } from '../../shared/models/order.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/orders`;

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.apiUrl, request);
  }

  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/my`);
  }

  getAll(): Observable<Order[]> {
    return this.http.get<Order[]>(this.apiUrl);
  }

  updateStatus(id: number, status: string): Observable<Order> {
    return this.http.put<Order>(`${this.apiUrl}/${id}/status`, { status });
  }
}
