# COSMETICO — Magazin de cosmetice (API + Angular)

API REST pentru un magazin de cosmetice, construit pe **ASP.NET Core 8 (Web API)** cu arhitectura pe straturi: `Controller → Service → Repository → EF Core / SQL Server`, plus **client Angular 20** (`cosmetico-client/`).

## Rulare cu Docker (recomandat)

Un container pentru API + un container pentru SQL Server, care comunică prin rețeaua Docker:

```
docker compose up --build
```

- Swagger: http://localhost:8080/swagger
- SQL Server expus pe `localhost,1433` (user `sa`, parola `CosmeticoSql!2026`) — se poate conecta din SSMS / Azure Data Studio.
- La pornire se aplică automat migrările și seed-ul (roluri, cont de admin, catalog).

Oprire: `docker compose down` (datele persistă în volumul `sqldata`).
Pentru reset complet al bazei: `docker compose down -v`.

## Rulare locală (fără Docker)

Necesită SQL Server LocalDB (inclus cu Visual Studio).

```
dotnet run
```

Swagger: http://localhost:5080/swagger

## Frontend Angular (cosmetico-client)

Necesită Node.js LTS. API-ul trebuie să ruleze (Docker pe `localhost:8080` sau `dotnet run` pe `localhost:5080` — vezi `src/environments/environment.ts`).

```
cd cosmetico-client
npm install
npm start
```

Aplicația: http://localhost:4200

### Pagini

| Ruta | Descriere | Acces |
|---|---|---|
| `/` | Catalog: căutare, filtrare pe categorie, sortare, paginare | public |
| `/products/:id` | Detalii produs: ingrediente, recenzii, adaugă în coș | public |
| `/login`, `/register` | Autentificare / înregistrare | public |
| `/cart` | Coș (persistat în localStorage) + plasare comandă | public / login la finalizare |
| `/orders/my` | Comenzile mele | autentificat |
| `/admin/products` | Admin: tabel + formular create/edit produse | Admin |
| `/admin/orders` | Admin: lista comenzi + schimbare status | Admin |

### Structura frontend-ului

```
cosmetico-client/src/app/
├── core/       — servicii (auth, cart, product, order...), guard-uri, interceptoare
├── shared/     — modele TypeScript (oglindă peste DTO-urile API) + utilitare
├── features/   — paginile, grupate pe funcționalitate (auth, catalog, cart, orders, admin)
├── app.routes.ts
└── app.config.ts
```

- `jwt.interceptor.ts` atașează automat `Authorization: Bearer ...`
- `error.interceptor.ts` face logout + redirect la login la 401
- `auth.guard` / `admin.guard` protejează rutele
- formulare reactive cu validare pe client (aceleași reguli ca pe server)

## Conturi seed

| Email | Parola | Rol |
|---|---|---|
| admin@cosmetico.ro | Admin123! | Admin |

Orice înregistrare nouă primește rolul **User**.

## Endpoint-uri principale

| Metodă | Ruta | Acces |
|---|---|---|
| POST | /api/auth/register, /api/auth/login | public |
| GET | /api/products (paginare, filtrare, sortare), /api/products/{id} | public |
| POST/PUT/DELETE | /api/products | Admin |
| GET | /api/categories, /api/brands | public |
| POST/PUT/DELETE | /api/categories, /api/brands | Admin |
| GET | /api/products/{id}/reviews | public |
| POST | /api/reviews | autentificat |
| DELETE | /api/reviews/{id} | proprietar sau Admin |
| POST | /api/orders | autentificat |
| GET | /api/orders/my | autentificat |
| GET | /api/orders, PUT /api/orders/{id}/status | Admin |
| GET | /api/ingredients | public |

## Structura proiectului

```
Controllers/    — traducere HTTP (fara logica de business)
Services/       — reguli de business (lucrand exclusiv cu DTO-uri)
Repositories/   — acces la date (generic + specifice)
Models/         — entitatile EF Core + Identity
DTOs/           — contracte de date (intrare/iesire)
Mapping/        — extensii entitate <-> DTO
Data/           — AppDbContext + SeedData
Middleware/     — exception handling global (ProblemDetails)
Exceptions/     — AppException, NotFound, BusinessRule, Forbidden
```

## Cerinte acoperite

- 6 controllere REST (min. 5 cerut)
- 7+ entitati; One-to-Many (Category→Product etc.) si Many-to-Many (Product↔Ingredient)
- Migrari EF Core (`InitialCreate` aplica tot: Identity + catalog)
- Arhitectura pe straturi cu DI (DTO / Service / Repository)
- Auth & Authorization: Identity + JWT, roluri Admin/User
- Exception handling global cu ProblemDetails (404/400/403, nu 500)
- Paginare, filtrare, sortare pe catalog
- Logica de comanda: snapshot de pret, total calculat pe server, verificare stoc, tranzactie unica
- Frontend Angular: 8 pagini, core/shared/features, auth JWT (interceptor + guard-uri),
  rute protejate, formulare reactive cu validare, coș persistent
