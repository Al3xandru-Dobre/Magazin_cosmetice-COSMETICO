# COSMETICO — Magazin de cosmetice (API)

API REST pentru un magazin de cosmetice, construit pe **ASP.NET Core 8 (Web API)** cu arhitectura pe straturi: `Controller → Service → Repository → EF Core / SQL Server`.

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
