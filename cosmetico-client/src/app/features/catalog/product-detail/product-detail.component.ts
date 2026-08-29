import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { ReviewService } from '../../../core/services/review.service';
import { ProductDetail } from '../../../shared/models/product.models';
import { Review } from '../../../shared/models/review.models';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-product-detail',
  imports: [DatePipe, DecimalPipe, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './product-detail.component.html',
})
export class ProductDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly reviewService = inject(ReviewService);
  private readonly cart = inject(CartService);
  protected readonly auth = inject(AuthService);

  protected readonly product = signal<ProductDetail | null>(null);
  protected readonly reviews = signal<Review[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly reviewError = signal<string | null>(null);
  protected readonly reviewSuccess = signal<string | null>(null);
  protected quantity = 1;

  protected readonly reviewForm = this.fb.nonNullable.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]],
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadProduct(id);
    this.loadReviews(id);
  }

  private loadProduct(id: number): void {
    this.productService.getById(id).subscribe({
      next: (product) => this.product.set(product),
      error: (err) => this.error.set(extractApiError(err)),
    });
  }

  private loadReviews(id: number): void {
    this.reviewService.getByProduct(id).subscribe({
      next: (reviews) => this.reviews.set(reviews),
      error: (err) => this.error.set(extractApiError(err)),
    });
  }

  addToCart(): void {
    const product = this.product();
    if (!product) return;
    this.cart.add(product, this.quantity);
  }

  stars(rating: number): string {
    return '\u2605'.repeat(Math.round(rating)) + '\u2606'.repeat(5 - Math.round(rating));
  }

  submitReview(): void {
    const product = this.product();
    if (!product || this.reviewForm.invalid) {
      this.reviewForm.markAllAsTouched();
      return;
    }

    this.reviewError.set(null);
    this.reviewSuccess.set(null);

    const { rating, comment } = this.reviewForm.getRawValue();
    this.reviewService.create({ productId: product.id, rating, comment }).subscribe({
      next: () => {
        this.reviewSuccess.set('Recenzie adaugata. Multumim!');
        this.reviewForm.reset({ rating: 5, comment: '' });
        this.loadReviews(product.id);
      },
      error: (err) => this.reviewError.set(extractApiError(err)),
    });
  }
}
