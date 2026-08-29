import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/catalog/catalog/catalog.component').then((m) => m.CatalogComponent),
    title: 'COSMETICO — Catalog',
  },
  {
    path: 'products/:id',
    loadComponent: () =>
      import('./features/catalog/product-detail/product-detail.component').then((m) => m.ProductDetailComponent),
    title: 'COSMETICO — Detalii produs',
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    title: 'COSMETICO — Autentificare',
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
    title: 'COSMETICO — Inregistrare',
  },
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart.component').then((m) => m.CartComponent),
    title: 'COSMETICO — Cos cumparaturi',
  },
  {
    path: 'orders/my',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/my-orders/my-orders.component').then((m) => m.MyOrdersComponent),
    title: 'COSMETICO — Comenzile mele',
  },
  {
    path: 'admin/products',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/admin-products/admin-products.component').then((m) => m.AdminProductsComponent),
    title: 'COSMETICO — Admin: produse',
  },
  {
    path: 'admin/orders',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./features/admin/admin-orders/admin-orders.component').then((m) => m.AdminOrdersComponent),
    title: 'COSMETICO — Admin: comenzi',
  },
  { path: '**', redirectTo: '' },
];
