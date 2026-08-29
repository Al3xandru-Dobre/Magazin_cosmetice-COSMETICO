import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../shared/models/paged-result.model';
import { Product, ProductDetail, ProductQuery, ProductPayload, UpdateProductPayload } from '../../shared/models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/products`;

  getPaged(query: ProductQuery): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', query.page ?? 1)
      .set('pageSize', query.pageSize ?? 12);

    if (query.search) params = params.set('search', query.search);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.brandId) params = params.set('brandId', query.brandId);
    if (query.minPrice != null) params = params.set('minPrice', query.minPrice);
    if (query.maxPrice != null) params = params.set('maxPrice', query.maxPrice);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);

    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  getById(id: number): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.apiUrl}/${id}`);
  }

  create(payload: ProductPayload): Observable<ProductDetail> {
    return this.http.post<ProductDetail>(this.apiUrl, payload);
  }

  update(id: number, payload: UpdateProductPayload): Observable<ProductDetail> {
    return this.http.put<ProductDetail>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
