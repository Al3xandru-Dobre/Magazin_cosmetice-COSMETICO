using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Data;

/// <summary>
/// Mosteneste IdentityDbContext, NU DbContext.
/// Asta aduce automat tabelele AspNetUsers, AspNetRoles, AspNetUserRoles etc.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // OBLIGATORIU si primul. Fara el, tabelele Identity nu se configureaza
        // si migrarea va esua sau va genera o schema incompleta.
        base.OnModelCreating(builder);

        // ---- Precizie zecimala ----
        // Fara asta EF Core mapeaza decimal la decimal(18,2) cu warning,
        // sau trunchiaza silentios in unele providere.
        builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
        builder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);

        // LineTotal este proprietate calculata in C#, nu coloana in DB.
        builder.Entity<OrderItem>().Ignore(oi => oi.LineTotal);

        // ---- Many-to-Many explicit: Product <-> Ingredient ----
        // EF Core ar deduce singur relatia, dar o configuram explicit
        // ca sa controlam numele tabelei de jonctiune.
        builder.Entity<Product>()
            .HasMany(p => p.Ingredients)
            .WithMany(i => i.Products)
            .UsingEntity(j => j.ToTable("ProductIngredients"));

        // ---- Comportament la stergere ----
        // Restrict: nu poti sterge o categorie care are produse.
        // Altfel EF ar sterge in cascada tot catalogul din acea categorie.
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Product>()
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: stergerea unei comenzi sterge si liniile ei. Liniile
        // nu au sens independent de comanda.
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restrict: nu stergem un produs care apare in comenzi -> folosim IsActive.
        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Indecsi ----
        // Cautarea si filtrarea din catalog lovesc aceste coloane la fiecare request.
        builder.Entity<Product>().HasIndex(p => p.Name);
        builder.Entity<Product>().HasIndex(p => p.CategoryId);
        builder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        builder.Entity<Brand>().HasIndex(b => b.Name).IsUnique();
        builder.Entity<Ingredient>().HasIndex(i => i.Name).IsUnique();

        // Un utilizator lasa o singura recenzie per produs.
        builder.Entity<Review>().HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
    }
}

