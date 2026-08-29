import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { CartService } from '../../../core/services/cart.service';
import { CategoryService } from '../../../core/services/category.service';
import { ProductService } from '../../../core/services/product.service';
import { Category } from '../../../shared/models/category.model';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { Product } from '../../../shared/models/product.models';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-catalog',
  imports: [FormsModule, RouterLink, DecimalPipe],
  templateUrl: './catalog.component.html',
})
export class CatalogComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly cart = inject(CartService);

  protected readonly result = signal<PagedResult<Product> | null>(null);
  protected readonly categories = signal<Category[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  protected search = '';
  protected categoryId: number | null = null;
  protected sortBy = 'name';
  protected page = 1;
  protected readonly pageSize = 12;

  /// ImagePath e relativ (/images/...); ii prefixam baza serverului.
  protected readonly serverUrl = environment.apiUrl.replace('/api', '');

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.productService
      .getPaged({
        page: this.page,
        pageSize: this.pageSize,
        search: this.search || null,
        categoryId: this.categoryId,
        sortBy: this.sortBy,
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(extractApiError(err));
          this.loading.set(false);
        },
      });
  }

  onFiltersChange(): void {
    this.page = 1;
    this.load();
  }

  goToPage(page: number): void {
    this.page = page;
    this.load();
  }

  pages(): number[] {
    const total = this.result()?.totalPages ?? 0;
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  stars(rating: number): string {
    return '\u2605'.repeat(Math.round(rating)) + '\u2606'.repeat(5 - Math.round(rating));
  }

  addToCart(product: Product): void {
    this.cart.add({
      id: product.id,
      name: product.name,
      price: product.price,
      stockQuantity: product.stockQuantity,
    });
  }
}
