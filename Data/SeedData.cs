using Magazin_cosmetice_COSMETICO.Models.Entities;
using Magazin_cosmetice_COSMETICO.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Magazin_cosmetice_COSMETICO.Data;

/// <summary>
/// Seeding la pornire: aplica migrarile, creeaza rolurile Admin/User,
/// contul de administrator si datele de catalog (3 categorii, 4 branduri,
/// 8 ingrediente, 15 produse). Idempotent: ruleaza doar ce lipseste.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
        var context = provider.GetRequiredService<AppDbContext>();

        // In Docker, containerul SQL Server poate sa nu fie inca gata cand
        // porneste API-ul, desi healthcheck-ul scutește de cele mai multe ori.
        // Reincercam migrarea de cateva ori inainte sa renuntam.
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                break;
            }
            catch (Exception ex) when (attempt < 5)
            {
                logger.LogWarning(ex,
                    "Baza de date nu e disponibila (incercarea {Attempt}/5). Reincerc in 10 secunde...",
                    attempt);
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        await SeedRolesAsync(provider, logger);
        var admin = await SeedAdminAsync(provider, logger);
        await SeedCatalogAsync(context, logger, admin);
    }

    private static async Task SeedRolesAsync(IServiceProvider provider, ILogger logger)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Rol creat: {Role}", role);
            }
        }
    }

    private static async Task<ApplicationUser> SeedAdminAsync(IServiceProvider provider, ILogger logger)
    {
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "admin@cosmetico.ro";
        var admin = await userManager.FindByEmailAsync(email);

        if (admin is not null)
            return admin;

        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Administrator COSMETICO",
            RegisteredAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");
        if (!result.Succeeded)
        {
            logger.LogError("Contul de admin nu a putut fi creat: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return admin;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
        logger.LogInformation("Cont de admin creat: {Email} / Admin123!", email);
        return admin;
    }

    private static async Task SeedCatalogAsync(AppDbContext context, ILogger logger, ApplicationUser admin)
    {
        if (await context.Products.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new() { Name = "Ingrijire ten", Description = "Seruri, creme si tratamente pentru un ten sanatos." },
            new() { Name = "Machiaj", Description = "Produse de machiaj pentru orice ocazie." },
            new() { Name = "Ingrijire par", Description = "Sampoane, balsamuri si tratamente capilare." }
        };

        var brands = new List<Brand>
        {
            new() { Name = "GlowRx", Country = "Coreea de Sud" },
            new() { Name = "Lumiere", Country = "Franta" },
            new() { Name = "NatureLab", Country = "Germania" },
            new() { Name = "VelvetTouch", Country = "Marea Britanie" }
        };

        var ingredients = new List<Ingredient>
        {
            new() { Name = "Retinol", IsAllergen = true },
            new() { Name = "Niacinamida", IsAllergen = false },
            new() { Name = "Acid hialuronic", IsAllergen = false },
            new() { Name = "Vitamina C", IsAllergen = false },
            new() { Name = "Acid salicilic", IsAllergen = true },
            new() { Name = "Glicerina", IsAllergen = false },
            new() { Name = "Zinc PCA", IsAllergen = false },
            new() { Name = "Ulei de arbore de ceai", IsAllergen = true }
        };

        // (nume, descriere, pret, stoc, indexCategorie, indexBrand, indexIngrediente)
        var products = new List<(string Name, string Desc, decimal Price, int Stock, int Cat, int Brand, int[] Ing)>
        {
            ("Ser Retinol Noapta GlowRx",
             "Ser concentrat cu retinol encapsulat pentru regenerarea tenului in timpul noptii.", 149.99m, 25, 0, 0, [0, 5]),
            ("Crema Hidratanta Acid Hialuronic",
             "Hidratare intensa 48 de ore cu acid hialuronic cu trei greutati moleculare.", 89.50m, 40, 0, 2, [2, 5]),
            ("Ser Vitamina C 15%",
             "Ser iluminator cu vitamina C stabilizata pentru un ten uniform si luminos.", 129.00m, 30, 0, 1, [3]),
            ("Gel de Curatare cu Acid Salicilic",
             "Gel delicat pentru curatarea tenului cu tendinta acneica, cu acid salicilic 2%.", 65.00m, 50, 0, 0, [4, 5]),
            ("Crema Niacinamida si Zinc",
             "Crema matifianta cu niacinamida 10% si zinc pentru controlul excesului de sebum.", 75.00m, 35, 0, 0, [1, 6]),
            ("Fond de Ten Velvet Matte",
             "Fond de ten cu finisaj mat si acoperire medie, rezistent pana la 12 ore.", 110.00m, 20, 1, 3, []),
            ("Rimel Volum Instant",
             "Rimel cu perie inovatoare pentru volum maxim si definitie, fara aglomerari.", 85.00m, 45, 1, 3, []),
            ("Paleta de Culori Nude Lumiere",
             "Paleta cu 12 nuante nude matte si satoase, pigmente profesionale.", 210.00m, 15, 1, 1, []),
            ("Ruj Satin NatureLab",
             "Ruj cu finisaj satinat, formula hranitoare cu uleiuri naturale.", 59.99m, 60, 1, 2, []),
            ("Baza de Machiaj Iluminatoare",
             "Primer iluminator care uniformizeaza tenul si prelungeste rezistenta machiajului.", 95.00m, 25, 1, 1, [2]),
            ("Sampon Regenerator cu Biotina",
             "Sampon fara sulfati cu biotina pentru par subtire, densitate vizibila.", 49.99m, 70, 2, 2, [6]),
            ("Masca Capilara Reparatoare",
             "Masca intensiva cu keratina pentru parul deteriorat termic si chimic.", 78.00m, 30, 2, 3, [2]),
            ("Ser pentru Varfuri Despicate",
             "Ser usor cu pantenol ce sigileaza varfurile si confera stralucire.", 92.00m, 20, 2, 0, [5]),
            ("Ulei de Par cu Argan",
             "Ulei de argan pur, presat la rece, pentru nutritie si protectie termica.", 105.50m, 18, 2, 1, []),
            ("Balsam Antimasca cu Ulei de Ceai",
             "Balsam purifiant cu ulei de arbore de ceai pentru scalp iritat.", 55.00m, 40, 2, 2, [7])
        };

        var productEntities = products.Select(p => new Product
        {
            Name = p.Name,
            Description = p.Desc,
            Price = p.Price,
            StockQuantity = p.Stock,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 90)),
            IsActive = true
        }).ToList();

        // Mai intai categoriile/brandurile/ingredientele, ca sa existe cheile,
        // apoi legam produsele dupa Id-urile reale generate de baza de date.
        context.Categories.AddRange(categories);
        context.Brands.AddRange(brands);
        context.Ingredients.AddRange(ingredients);
        await context.SaveChangesAsync();

        for (var i = 0; i < productEntities.Count; i++)
        {
            productEntities[i].CategoryId = categories[products[i].Cat].Id;
            productEntities[i].BrandId = brands[products[i].Brand].Id;
            productEntities[i].Ingredients = products[i].Ing
                .Select(idx => ingredients[idx])
                .ToList();
        }

        context.Products.AddRange(productEntities);
        await context.SaveChangesAsync();

        var reviewSamples = new (int ProductIdx, int Rating, string Comment)[]
        {
            (0, 5, "Cel mai bun ser de retinol pe care l-am folosit. Tenul arata mult mai bine."),
            (1, 4, "Hidratare excelenta, absorbtie rapida. Il recomand pentru tenul uscat."),
            (2, 5, "Dupa doua saptamani, petele s-au vizibil estompat. Miros placut de citrice."),
            (7, 4, "Nuante superbe, pigmentare buna. Paleta e compacta, perfecta pentru voiaj."),
            (10, 3, "Parul se calculeaza mai usor, dar pentru volum asteptam mai mult.")
        };

        context.Reviews.AddRange(reviewSamples.Select(r => new Review
        {
            ProductId = productEntities[r.ProductIdx].Id,
            UserId = admin.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
        }));

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Catalog seeded: {Categories} categorii, {Brands} branduri, {Ingredients} ingrediente, {Products} produse.",
            categories.Count, brands.Count, ingredients.Count, productEntities.Count);
    }
}
