# Plan de dezvoltare — GlowUp (magazin de cosmetice)

**Țintă:** 40p (plafonul maxim), din 50p disponibili.
**Strategie:** obligatoriile (22p) + Architecture (5p) + Auth backend (5p) + Auth frontend (4p) + Folder structure FE (5p) = **41p**, cu 1p marjă de siguranță.

---

## Principiul de bază: vertical slices

Nu construi orizontal (toate entitățile → toate repository-urile → toate serviciile → toate controllerele). Construiește **vertical**: o funcționalitate completă de la bază de date până la ecran, apoi următoarea.

De ce contează: dacă lucrezi orizontal și descoperi în săptămâna 5 că arhitectura are o problemă, ai 6 entități de refăcut. Dacă lucrezi vertical, descoperi problema după primul slice și o repari o singură dată.

**Regula de aur:** după fiecare fază, aplicația trebuie să pornească și să facă ceva vizibil. Fără „merge doar când termin tot".

---

## Faza 0 — Fundația (2–3 zile)

**Obiectiv:** proiectul pornește, baza de date există, Swagger răspunde.

| Pas | Ce faci | Verificare |
|---|---|---|
| 0.1 | Proiect nou `ASP.NET Core Web API`, .NET 8, cu **Use controllers** bifat | `dotnet run` → Swagger se deschide |
| 0.2 | Instalezi pachetele din `PACHETE_NUGET.txt` | Build fără erori |
| 0.3 | Git repo + `.gitignore` pentru .NET | `git status` nu listează `bin/`, `obj/` |
| 0.4 | Copiezi toate entitățile din `Models/` | Build OK |
| 0.5 | `AppDbContext` + connection string în `appsettings.Development.json` | — |
| 0.6 | `Add-Migration InitialCreate` → `Update-Database` | Vezi tabelele în SQL Server Object Explorer |
| 0.7 | `SeedData.cs`: 3 categorii, 4 branduri, 8 ingrediente, 15 produse | Datele apar la View Data |

**Definition of done:** deschizi `dbo.Products` în SQL Server Object Explorer și vezi 15 rânduri. Tabela `ProductIngredients` există și are rânduri.

> **Capcana #1:** nu uita `base.OnModelCreating(builder)` ca **prima linie**. Fără ea, tabelele Identity nu se generează și migrarea produce o schemă incompletă pe care o descoperi abia peste două faze.

> **Capcana #2:** nu trece mai departe cu schema greșită. Refactorizarea modelului de date în faza 4 e cel mai scump lucru din tot proiectul, pentru că invalidează migrări, seed data, DTO-uri și componente frontend simultan.

---

## Faza 1 — Primul slice complet: Products (3–4 zile)

**Obiectiv:** șablonul pe care îl repeți apoi mecanic pentru toate celelalte entități.

Ordinea de scriere contează — se merge **de jos în sus**, pentru că fiecare strat depinde doar de cel de sub el:

1. `IRepository<T>` + `Repository<T>` — generic, scris o dată pentru totdeauna
2. `IProductRepository` + `ProductRepository` — filtrare, paginare, `Include`-uri
3. `ProductDtos.cs` — Create/Update/Read/QueryParameters
4. `ProductMappings.cs` — extension methods entitate ↔ DTO
5. `AppExceptions.cs` + `ExceptionHandlingMiddleware.cs`
6. `IProductService` + `ProductService` — regulile de business
7. `ProductsController` — doar traducere HTTP
8. Înregistrări în `Program.cs` (DI + middleware)

**Definition of done — testat manual în Swagger:**

- [ ] `GET /api/products` → 200, listă paginată cu metadate corecte
- [ ] `GET /api/products?search=serum&categoryId=1&sortBy=price_desc` → filtrare funcțională
- [ ] `GET /api/products?page=2&pageSize=5` → pagina 2 diferă de pagina 1
- [ ] `GET /api/products/999` → **404** cu body `ProblemDetails`, nu 500
- [ ] `POST /api/products` cu `name` de 2 caractere → **400** cu mesajul de validare
- [ ] `POST /api/products` valid → **201** + header `Location`
- [ ] `DELETE /api/products/{id}` → **204**

> **Cel mai important test din tot proiectul:** cel cu 404-ul. Dacă primești 500, middleware-ul nu e înregistrat primul în pipeline sau nu prinde `AppException`. Repară-l acum, cât ai un singur controller de verificat.

---

## Faza 2 — Autentificare și autorizare (3–4 zile)

Se face **devreme**, nu la final. Motiv: `[Authorize]` schimbă comportamentul tuturor controllerelor, iar frontend-ul are nevoie de token pentru orice request protejat. Dacă îl amâni, refaci integrarea de două ori.

| Pas | Ce faci |
|---|---|
| 2.1 | `ApplicationUser : IdentityUser`, `AppDbContext : IdentityDbContext<ApplicationUser>` |
| 2.2 | `Add-Migration AddIdentity` → `Update-Database` |
| 2.3 | `ITokenService` / `TokenService` — generează JWT cu claim-urile `NameIdentifier`, `Email`, `Role` |
| 2.4 | `IAuthService` / `AuthService` — register, login |
| 2.5 | `AuthController` — `POST /api/auth/register`, `POST /api/auth/login` |
| 2.6 | `SeedData`: rolurile `Admin` + `User`, plus un cont de admin |
| 2.7 | `[Authorize(Roles = "Admin")]` pe POST/PUT/DELETE din `ProductsController` |
| 2.8 | Swagger cu buton **Authorize** (deja în `Program.cs`) |

**Definition of done:**

- [ ] Register → 201, userul apare în `AspNetUsers`
- [ ] Login → 200 cu token JWT; îl decodezi pe jwt.io și vezi claim-ul de rol
- [ ] `POST /api/products` **fără** token → **401**
- [ ] `POST /api/products` cu token de **User** → **403**
- [ ] `POST /api/products` cu token de **Admin** → **201**

> **Capcana #3:** `app.UseAuthentication()` trebuie să fie **înaintea** lui `app.UseAuthorization()`. Inversate, orice `[Authorize]` respinge totul, pentru că la momentul verificării rolului identitatea nu e încă stabilită. Simptom tipic: 401 chiar și cu token valid.

> **Capcana #4:** cheia JWT trebuie să aibă minimum 32 de caractere (256 biți pentru HMAC-SHA256). Sub asta, `SymmetricSecurityKey` aruncă excepție la pornire.

---

## Faza 3 — Restul backend-ului (4–5 zile)

Acum repeți șablonul din Faza 1. Fiecare entitate durează progresiv mai puțin.

| Controller | Endpoint-uri | Complexitate |
|---|---|---|
| `CategoriesController` | CRUD complet | Trivial — copiază Products, scoate filtrarea |
| `BrandsController` | CRUD complet | Trivial |
| `ReviewsController` | `GET /api/products/{id}/reviews`, `POST`, `DELETE` | Medie — verificare de proprietar |
| `OrdersController` | `POST`, `GET /my`, `GET` (admin), `PUT /{id}/status` (admin) | **Ridicată** |

### `OrderService` — singura logică netrivială din proiect

Lasă-l la final, când ești deja obișnuit cu straturile. Regulile:

1. Validează că toate produsele există și sunt `IsActive`
2. Verifică stocul pentru fiecare linie → altfel `BusinessRuleException`
3. Copiază prețul curent în `OrderItem.UnitPrice` (**snapshot**)
4. Calculează `TotalAmount` **pe server**, ignorând orice valoare din request
5. Scade `StockQuantity`
6. Un singur `SaveChangesAsync()` la final → totul într-o tranzacție

> **De ce un singur SaveChanges:** dacă salvezi comanda și abia apoi scazi stocul, o eroare între cele două lasă baza într-o stare inconsistentă — comandă existentă, stoc nescăzut. Change tracker-ul EF Core adună toate modificările și le trimite ca o singură tranzacție.

**Reguli pentru `ReviewsController`:**
- Un user își poate șterge doar propria recenzie; adminul le poate șterge pe toate → `ForbiddenException`
- Indexul unic `(ProductId, UserId)` blochează recenziile duplicate la nivel de bază de date

**Definition of done:** toate cele 6 controllere răspund corect în Swagger. Testează în special comanda cu cantitate mai mare decât stocul → 400, nu 500.

---

## Faza 4 — Frontend: fundația (3–4 zile)

Nu începe cu paginile. Începe cu infrastructura, altfel rescrii fiecare componentă când adaugi interceptorul.

```
ng new glowup-client --routing --style=scss
```

| Pas | Ce faci |
|---|---|
| 4.1 | Structura `core/` `shared/` `features/` (cei 5p de folder structure) |
| 4.2 | Modele TypeScript oglindă pentru DTO-uri |
| 4.3 | `environment.ts` cu `apiUrl: 'https://localhost:7xxx/api'` |
| 4.4 | `auth.service.ts` — login, register, `getToken()`, `isAdmin()` |
| 4.5 | `jwt.interceptor.ts` — atașează `Authorization: Bearer ...` automat |
| 4.6 | `error.interceptor.ts` — 401 → redirect la login |
| 4.7 | `auth.guard.ts`, `admin.guard.ts` |
| 4.8 | `navbar` + layout de bază |

> **Capcana #5:** CORS. Dacă browserul dă `blocked by CORS policy`, verifică în `Program.cs` că `.WithOrigins("http://localhost:4200")` are portul exact și că `app.UseCors()` e **înaintea** lui `UseAuthentication()`.

**Definition of done:** te loghezi din Angular, tokenul ajunge în localStorage, iar un request către un endpoint protejat trece.

---

## Faza 5 — Frontend: paginile (4–5 zile)

Ordinea urcă în complexitate:

1. **Login** + **Register** — formulare reactive cu validare
2. **Catalog** — listă, filtrare pe categorie, paginare, căutare
3. **Detalii produs** — imagine, ingrediente, recenzii
4. **Coș** — stare în `CartService`, persistat în localStorage
5. **Comenzile mele**
6. **Admin: produse** — tabel + formular create/edit
7. **Admin: comenzi** — listă + schimbare status

**Validări pe formulare (2p):** aceleași reguli ca DataAnnotations de pe DTO-uri. Client pentru UX, server pentru securitate — **niciodată doar client**. Un `curl` ocolește orice validare Angular.

**Definition of done:** parcurgi fluxul complet fără să atingi Swagger — te înregistrezi, cauți un produs, îl adaugi în coș, plasezi comanda, o vezi în „Comenzile mele".

---

## Faza 6 — Finisaje (2 zile)

| Task | Punctaj | Efort |
|---|---|---|
| Serilog cu sink de fișier | 2p | 30 min |
| Logging în servicii (`LogInformation` la operațiile de scriere) | — | 1h |
| Upload imagini produs (`IFormFile`, ca în Lab 5) | — | 2h |
| `README.md` — cum se rulează, cont de admin, screenshot-uri | — | 1h |
| Verificare finală a listei de cerințe | — | 30 min |

---

## Checklist final pe cerințe

**Obligatorii (fără ele se scad puncte):**
- [ ] Minimum 5 controllere REST → *ai 6*
- [ ] Minimum 6 entități → *ai 9*
- [ ] One-to-Many → *Category→Product, Brand→Product, User→Order, Order→OrderItem, Product→Review*
- [ ] Many-to-Many → *Product↔Ingredient*
- [ ] Migrations folosite → *InitialCreate, AddIdentity, ...*
- [ ] Minimum 5 pagini frontend → *ai 8*
- [ ] API integration
- [ ] Frontend nu e Vanilla → *Angular*

**Opționale asumate:**
- [ ] Architecture 5p — DTO ✓ Service ✓ Repository ✓ DI ✓
- [ ] Auth & Authorization 5p — Identity ✓ roluri Admin+User ✓ **4** endpoint-uri Admin-only ✓
- [ ] Frontend auth 4p
- [ ] Folder structure 5p
- [ ] Exception handling 3p *(bonus — deja implementat în șablon)*
- [ ] Protected routes 2p
- [ ] Forms with validation 2p
- [ ] Logging 2p

---

## Estimare totală

| Fază | Durată |
|---|---|
| 0 — Fundația | 2–3 zile |
| 1 — Slice Products | 3–4 zile |
| 2 — Auth | 3–4 zile |
| 3 — Restul backend | 4–5 zile |
| 4 — Frontend fundație | 3–4 zile |
| 5 — Frontend pagini | 4–5 zile |
| 6 — Finisaje | 2 zile |
| **Total** | **21–27 zile de lucru efectiv** |

Cu 2–3 ore pe zi, asta înseamnă 5–7 săptămâni calendaristice. Fazele 0–2 sunt cele care decid dacă restul merge ușor sau greu; nu le grăbi.
