import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { CartItem } from '../../shared/models/cart.model';
import { ProductDetail } from '../../shared/models/product.models';

const STORAGE_KEY = 'cosmetico_cart';

/// Cosul traieste in memory (signal) si se persista in localStorage
/// la fiecare modificare, deci supravietuieste unui refresh.
@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly _items = signal<CartItem[]>(readStoredItems());

  readonly items = this._items.asReadonly();
  readonly count = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));
  readonly total = computed(() => this._items().reduce((sum, i) => sum + i.quantity * i.price, 0));

  constructor() {
    effect(() => {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this._items()));
    });
  }

  add(product: ProductDetail | { id: number; name: string; price: number; stockQuantity: number }, quantity = 1): void {
    this._items.update((items) => {
      const existing = items.find((i) => i.productId === product.id);
      if (existing) {
        return items.map((i) =>
          i.productId === product.id
            ? { ...i, quantity: Math.min(i.quantity + quantity, i.stockQuantity) }
            : i,
        );
      }
      return [
        ...items,
        {
          productId: product.id,
          name: product.name,
          price: product.price,
          quantity: Math.min(quantity, product.stockQuantity),
          stockQuantity: product.stockQuantity,
        },
      ];
    });
  }

  updateQuantity(productId: number, quantity: number): void {
    this._items.update((items) =>
      items.map((i) =>
        i.productId === productId
          ? { ...i, quantity: Math.max(1, Math.min(quantity, i.stockQuantity)) }
          : i,
      ),
    );
  }

  remove(productId: number): void {
    this._items.update((items) => items.filter((i) => i.productId !== productId));
  }

  clear(): void {
    this._items.set([]);
  }
}

function readStoredItems(): CartItem[] {
  try {
    return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]') as CartItem[];
  } catch {
    return [];
  }
}
