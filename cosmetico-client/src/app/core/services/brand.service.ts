import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Brand } from '../../shared/models/brand.model';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/brands`;

  getAll(): Observable<Brand[]> {
    return this.http.get<Brand[]>(this.apiUrl);
  }
}
