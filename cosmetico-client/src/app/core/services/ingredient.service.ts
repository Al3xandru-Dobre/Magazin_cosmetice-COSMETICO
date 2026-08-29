import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Ingredient } from '../../shared/models/ingredient.model';

@Injectable({ providedIn: 'root' })
export class IngredientService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/ingredients`;

  getAll(): Observable<Ingredient[]> {
    return this.http.get<Ingredient[]>(this.apiUrl);
  }
}
