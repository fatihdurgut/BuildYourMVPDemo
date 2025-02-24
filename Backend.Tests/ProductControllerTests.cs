using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Backend.Tests;

public class ProductControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestProductDb")
            .Options;

        _context = new ApplicationDbContext(options);
        SeedTestData();
        _controller = new ProductController(_context);
    }

    private void SeedTestData()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var category = new Category { CategoryId = 1, Name = "Test Category" };
        _context.Categories.Add(category);

        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Test Product 1", Price = 10.99m, CategoryId = 1 },
            new Product { ProductId = 2, Name = "Test Product 2", Price = 20.99m, CategoryId = 1 }
        };
        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetProducts_ReturnsAllProducts()
    {
        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.Equal(2, products.Count());
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsProduct()
    {
        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        var product = Assert.IsType<Product>(actionResult.Value);
        Assert.Equal(1, product.ProductId);
        Assert.Equal("Test Product 1", product.Name);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsProductsInCategory()
    {
        // Act
        var result = await _controller.GetProductsByCategory(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.Equal(2, products.Count());
        Assert.All(products, product => Assert.Equal(1, product.CategoryId));
    }

    [Fact]
    public async Task GetProducts_WhenDatabaseEmpty_ReturnsEmptyList()
    {
        // Arrange
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.Empty(products);
    }

    [Fact]
    public async Task GetProducts_IncludesCategories()
    {
        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.All(products, product => Assert.NotNull(product.Category));
    }

    [Theory]
    [InlineData(1, "Test Product 1", true)]
    [InlineData(999, null, false)]
    public async Task GetProduct_ReturnsExpectedResult(int productId, string? expectedName, bool shouldExist)
    {
        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        if (shouldExist)
        {
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal(expectedName, product.Name);
            Assert.NotNull(product.Category);
        }
        else
        {
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
    }

    [Theory]
    [InlineData(1, 2)]     // Existing category with 2 products
    [InlineData(999, 0)]   // Non-existent category
    public async Task GetProductsByCategory_ReturnsExpectedCount(int categoryId, int expectedCount)
    {
        // Act
        var result = await _controller.GetProductsByCategory(categoryId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.Equal(expectedCount, products.Count());
        if (expectedCount > 0)
        {
            Assert.All(products, product => 
            {
                Assert.Equal(categoryId, product.CategoryId);
                Assert.NotNull(product.Category);
            });
        }
    }

    [Fact]
    public async Task GetProductsByCategory_WithMultipleCategories_ReturnCorrectProducts()
    {
        // Arrange
        var category2 = new Category { CategoryId = 2, Name = "Second Category" };
        _context.Categories.Add(category2);
        var product3 = new Product { ProductId = 3, Name = "Test Product 3", Price = 30.99m, CategoryId = 2 };
        _context.Products.Add(product3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProductsByCategory(2);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
        Assert.Single(products);
        var product = products.First();
        Assert.Equal("Test Product 3", product.Name);
        Assert.Equal(2, product.CategoryId);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}