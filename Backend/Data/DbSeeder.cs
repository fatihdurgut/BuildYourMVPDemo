using Backend.Models;

namespace Backend.Data;

public static class DbSeeder
{
    public static async Task SeedData(ApplicationDbContext context)
    {
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Tents",
                    Description = "High-quality camping tents for all your outdoor adventures",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Backpacks",
                    Description = "Durable backpacks for hiking and camping",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Clothing",
                    Description = "Essential outdoor clothing and gear",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            var products = new List<Product>
            {
                // Tents
                new Product
                {
                    Name = "TrailMaster X4 Tent",
                    Description = "Four-person camping tent with weather-resistant design",
                    Price = 200.00M,
                    ImageUrl = "/images/tent1.png",
                    CategoryId = categories[0].CategoryId,
                    StockQuantity = 10,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Alpine Explorer Tent",
                    Description = "Lightweight two-person tent for mountain camping",
                    Price = 300.00M,
                    ImageUrl = "/images/tent2.png",
                    CategoryId = categories[0].CategoryId,
                    StockQuantity = 15,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Sky View 2-Person Tent",
                    Description = "Tent with transparent roof panel for stargazing",
                    Price = 380.00M,
                    ImageUrl = "/images/tent3.png",
                    CategoryId = categories[0].CategoryId,
                    StockQuantity = 8,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Backpacks
                new Product
                {
                    Name = "Adventurer Pro Backpack",
                    Description = "Large capacity hiking backpack with multiple compartments",
                    Price = 60.00M,
                    ImageUrl = "/images/backpack1.png",
                    CategoryId = categories[1].CategoryId,
                    StockQuantity = 20,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Summit Climber Backpack",
                    Description = "Professional climbing backpack with gear attachments",
                    Price = 70.00M,
                    ImageUrl = "/images/backpack2.png",
                    CategoryId = categories[1].CategoryId,
                    StockQuantity = 25,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "TrailLite DayPack",
                    Description = "Lightweight daypack for short hiking trips",
                    Price = 40.00M,
                    ImageUrl = "/images/backpack3.png",
                    CategoryId = categories[1].CategoryId,
                    StockQuantity = 30,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Clothing
                new Product
                {
                    Name = "Summit Breeze Jacket",
                    Description = "Waterproof and breathable hiking jacket",
                    Price = 60.00M,
                    ImageUrl = "/images/jacket.png",
                    CategoryId = categories[2].CategoryId,
                    StockQuantity = 15,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Trail Blaze Pants",
                    Description = "Durable hiking pants with convertible legs",
                    Price = 80.00M,
                    ImageUrl = "/images/pants.png",
                    CategoryId = categories[2].CategoryId,
                    StockQuantity = 20,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "TREKSTAR Boots",
                    Description = "All-terrain hiking boots with ankle support",
                    Price = 120.00M,
                    ImageUrl = "/images/boots.png",
                    CategoryId = categories[2].CategoryId,
                    StockQuantity = 12,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}