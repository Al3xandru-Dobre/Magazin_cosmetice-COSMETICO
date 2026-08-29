import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Review, CreateReviewRequest } from '../../shared/models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly http = inject(HttpClient);

  getByProduct(productId: number): Observable<Review[]> {
    return this.http.get<Review[]>(`${environment.apiUrl}/products/${productId}/reviews`);
  }

  create(request: CreateReviewRequest): Observable<Review> {
    return this.http.post<Review>(`${environment.apiUrl}/reviews`, request);
  }
}
