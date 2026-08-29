import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BrandService } from '../../../core/services/brand.service';
import { CategoryService } from '../../../core/services/category.service';
import { IngredientService } from '../../../core/services/ingredient.service';
import { ProductService } from '../../../core/services/product.service';
import { Brand } from '../../../shared/models/brand.model';
import { Category } from '../../../shared/models/category.model';
import { Ingredient } from '../../../shared/models/ingredient.model';
import { PagedResult } from '../../../shared/models/paged-result.model';
import { Product, ProductDetail } from '../../../shared/models/product.models';
import { extractApiError } from '../../../shared/utils/api-error.util';

@Component({
  selector: 'app-admin-products',
  imports: [DecimalPipe, ReactiveFormsModule],
  templateUrl: './admin-products.component.html',
})
export class AdminProductsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly brandService = inject(BrandService);
  private readonly ingredientService = inject(IngredientService);

  protected readonly result = signal<PagedResult<Product> | null>(null);
  protected readonly categories = signal<Category[]>([]);
  protected readonly brands = signal<Brand[]>([]);
  protected readonly ingredients = signal<Ingredient[]>([]);

  protected readonly editingId = signal<number | null>(null);
  protected readonly showForm = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected selectedIngredientIds = new Set<number>();

  protected readonly selectedFile = signal<File | null>(null);
  protected readonly previewUrl = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]],
    price: [null as number | null, [Validators.required, Validators.min(0.01), Validators.max(100000)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    categoryId: [null as number | null, [Validators.required, Validators.min(1)]],
    brandId: [null as number | null, [Validators.required, Validators.min(1)]],
    isActive: [true],
  });

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((c) => this.categories.set(c));
    this.brandService.getAll().subscribe((b) => this.brands.set(b));
    this.ingredientService.getAll().subscribe((i) => this.ingredients.set(i));
    this.load();
  }

  load(): void {
    this.productService.getPaged({ page: 1, pageSize: 50, sortBy: 'name' }).subscribe({
      next: (result) => this.result.set(result),
      error: (err) => this.errorMessage.set(extractApiError(err)),
    });
  }

  startCreate(): void {
    this.editingId.set(null);
    this.selectedIngredientIds.clear();
    this.clearFileSelection();
    this.form.reset({ name: '', description: '', price: null, stockQuantity: 0, categoryId: null, brandId: null, isActive: true });
    this.showForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  startEdit(id: number): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.clearFileSelection();

    this.productService.getById(id).subscribe({
      next: (product: ProductDetail) => {
        this.editingId.set(id);
        this.selectedIngredientIds = new Set(product.ingredients.map((i) => i.id));
        this.form.reset({
          name: product.name,
          description: product.description,
          price: product.price,
          stockQuantity: product.stockQuantity,
          categoryId: product.categoryId,
          brandId: product.brandId,
          isActive: product.isActive,
        });
        this.showForm.set(true);
      },
      error: (err) => this.errorMessage.set(extractApiError(err)),
    });
  }

  /// Validare client identica cu cea de pe server: doar imagini, maxim 5 MB.
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type)) {
      this.errorMessage.set('Doar imagini JPG, PNG sau WEBP sunt acceptate.');
      input.value = '';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.errorMessage.set('Imaginea depaseste 5 MB.');
      input.value = '';
      return;
    }

    this.errorMessage.set(null);
    this.clearFileSelection();
    this.selectedFile.set(file);
    this.previewUrl.set(URL.createObjectURL(file));
  }

  clearFileSelection(): void {
    if (this.previewUrl()) {
      URL.revokeObjectURL(this.previewUrl()!);
    }
    this.selectedFile.set(null);
    this.previewUrl.set(null);
  }

  toggleIngredient(id: number, checked: boolean): void {
    if (checked) {
      this.selectedIngredientIds.add(id);
    } else {
      this.selectedIngredientIds.delete(id);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const payload = {
      name: raw.name,
      description: raw.description,
      price: raw.price!,
      stockQuantity: raw.stockQuantity,
      categoryId: raw.categoryId!,
      brandId: raw.brandId!,
      ingredientIds: [...this.selectedIngredientIds],
    };

    const editingId = this.editingId();
    const request$ = editingId
      ? this.productService.update(editingId, { ...payload, isActive: raw.isActive })
      : this.productService.create(payload);

    request$.subscribe({
      next: (product) => {
        // Produsul e salvat; urmeaza imaginea (daca e selectata) — POST /api/products/{id}/image.
        const file = this.selectedFile();
        if (file) {
          this.productService.uploadImage(product.id, file).subscribe({
            next: () => this.afterSave(product.name, editingId),
            error: (err) => {
              this.errorMessage.set(
                `Produsul a fost salvat, dar imaginea nu a putut fi incarcata: ${extractApiError(err)}`,
              );
              this.showForm.set(false);
              this.editingId.set(null);
              this.clearFileSelection();
              this.load();
            },
          });
        } else {
          this.afterSave(product.name, editingId);
        }
      },
      error: (err) => this.errorMessage.set(extractApiError(err)),
    });
  }

  private afterSave(name: string, editingId: number | null): void {
    this.successMessage.set(
      editingId ? `Produsul "${name}" a fost actualizat.` : `Produsul "${name}" a fost creat.`,
    );
    this.showForm.set(false);
    this.editingId.set(null);
    this.clearFileSelection();
    this.load();
  }

  remove(id: number, name: string): void {
    if (!confirm(`Sigur stergi produsul "${name}"? Produsele care apar in comenzi vor fi dezactivate, nu sterse.`)) {
      return;
    }

    this.productService.delete(id).subscribe({
      next: () => {
        this.successMessage.set('Produs sters/dezactivat.');
        this.load();
      },
      error: (err) => this.errorMessage.set(extractApiError(err)),
    });
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
  }
}
