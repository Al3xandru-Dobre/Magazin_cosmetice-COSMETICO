using System.Text;
using Magazin_cosmetice_COSMETICO.Data;
using Magazin_cosmetice_COSMETICO.Middleware;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Magazin_cosmetice_COSMETICO.Repositories;
using Magazin_cosmetice_COSMETICO.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1. SERVICII (containerul de Dependency Injection)
// =====================================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ---- Baza de date ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Identity ----
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---- Autentificare JWT ----
// De ce JWT si nu cookies? Frontend-ul Angular ruleaza pe alt origin (alt port).
// JWT este stateless: serverul nu tine sesiuni, tokenul se auto-valideaza.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key lipseste din configuratie.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // fara toleranta implicita de 5 minute la expirare
        };
    });

builder.Services.AddAuthorization();

// ---- Repositories ----
// Scoped = o instanta per request HTTP. Trebuie sa fie acelasi lifetime
// ca DbContext-ul, altfel un serviciu Singleton ar tine un DbContext
// disposed dupa primul request ("captive dependency").
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// ---- Services ----
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ---- CORS pentru frontend ----
const string FrontendPolicy = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendPolicy, policy => policy
        .WithOrigins("http://localhost:4200")  // Angular dev server
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---- Swagger cu suport pentru butonul Authorize ----
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "COSMETICO API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Introduceti tokenul JWT (fara prefixul 'Bearer ').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =====================================================================
// 2. PIPELINE-UL DE MIDDLEWARE
//    ORDINEA CONTEAZA. Pipeline-ul e o matrioska: ce inregistrezi
//    primul invaluie tot ce urmeaza.
// =====================================================================

// PRIMUL: prinde exceptii din orice middleware de mai jos.
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();            // serveste wwwroot/images

app.UseCors(FrontendPolicy);     // INAINTE de Authentication

app.UseAuthentication();         // CINE esti  -> valideaza tokenul
app.UseAuthorization();          // CE ai voie -> verifica rolurile
// Inversate, [Authorize] ar respinge orice request, pentru ca la momentul
// verificarii rolului identitatea nu ar fi inca stabilita.

app.MapControllers();

// Seed: aplica migrarile, creeaza rolurile Admin/User, contul de
// administrator si datele de catalog la pornire.
await SeedData.InitializeAsync(app.Services);

app.Run();
