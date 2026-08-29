import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../../shared/models/auth.models';

const STORAGE_KEY = 'cosmetico_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  private readonly _currentUser = signal<AuthResponse | null>(readStoredUser());

  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly isAdmin = computed(() => this._currentUser()?.roles.includes('Admin') ?? false);
  readonly displayName = computed(() => this._currentUser()?.fullName ?? '');

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/login`, request)
      .pipe(tap((response) => this.store(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiUrl}/register`, request)
      .pipe(tap((response) => this.store(response)));
  }

  getToken(): string | null {
    return this._currentUser()?.token ?? null;
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._currentUser.set(null);
  }

  private store(response: AuthResponse): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    this._currentUser.set(response);
  }
}

/// Restaureaza sesiunea din localStorage, dar IGNORA tokenurile expirate.
/// Altfel: guard-ul lasa userul pe o ruta protejata, pagina incepe sa se
/// randeze, API-ul raspunde 401 si abea atunci interceptorul il arunca la
/// login — un "flash" de pagina rupta. Verificarea e doar cosmetica (fara
/// acces la ceasul serverului); autoritatea ramane serverul, prin 401.
function readStoredUser(): AuthResponse | null {
  try {
    const stored = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? 'null') as AuthResponse | null;
    if (!stored) return null;

    if (new Date(stored.expiresAt) <= new Date()) {
      localStorage.removeItem(STORAGE_KEY);
      return null;
    }

    return stored;
  } catch {
    return null;
  }
}
