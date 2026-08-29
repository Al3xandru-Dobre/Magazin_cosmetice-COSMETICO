import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { extractApiError } from '../../shared/utils/api-error.util';

@Component({
  selector: 'app-cart',
  imports: [DecimalPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './cart.component.html',
})
export class CartComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  protected readonly cart = inject(CartService);
  private readonly orderService = inject(OrderService);
  protected readonly auth = inject(AuthService);

  protected readonly errorMessage = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    shippingAddress: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
  });

  increase(productId: number, quantity: number): void {
    this.cart.updateQuantity(productId, quantity + 1);
  }

  decrease(productId: number, quantity: number): void {
    this.cart.updateQuantity(productId, quantity - 1);
  }

  remove(productId: number): void {
    this.cart.remove(productId);
  }

  placeOrder(): void {
    // Cosul e public, dar plasarea necesita cont: redirect cu revenire dupa login.
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/cart' } });
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    const items = this.cart.items().map((i) => ({ productId: i.productId, quantity: i.quantity }));

    this.orderService.create({ shippingAddress: this.form.getRawValue().shippingAddress, items }).subscribe({
      next: () => {
        this.cart.clear();
        this.router.navigate(['/orders/my']);
      },
      error: (err) => {
        this.errorMessage.set(extractApiError(err));
        this.submitting.set(false);
      },
    });
  }
}
