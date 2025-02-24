using APIGateway.Controllers;
using APIGateway.Models;
using APIGateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace APIGateway.Tests;

public class ProductControllerTests
{
    private readonly Mock<IBackendService> _backendServiceMock;
    private readonly Mock<ILogger<ProductController>> _loggerMock;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _backendServiceMock = new Mock<IBackendService>();
        _loggerMock = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_backendServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetProducts_ReturnsOkResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Test Product 1" },
            new Product { ProductId = 2, Name = "Test Product 2" }
        };
        _backendServiceMock.Setup(x => x.GetProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
    }

    [Fact]
    public async Task GetProducts_WhenBackendThrows_Returns500()
    {
        // Arrange
        _backendServiceMock.Setup(x => x.GetProductsAsync())
            .ThrowsAsync(new BackendServiceException("Test error"));

        // Act
        var result = await _controller.GetProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetProduct_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var product = new Product { ProductId = 1, Name = "Test Product" };
        _backendServiceMock.Setup(x => x.GetProductByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProduct = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Test Product", returnedProduct.Name);
    }

    [Fact]
    public async Task GetProduct_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _backendServiceMock.Setup(x => x.GetProductByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProduct(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetProduct_WhenBackendThrows_Returns500()
    {
        // Arrange
        _backendServiceMock.Setup(x => x.GetProductByIdAsync(1))
            .ThrowsAsync(new BackendServiceException("Test error"));

        // Act
        var result = await _controller.GetProduct(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Product>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsOkResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Category Product 1", CategoryId = 1 },
            new Product { ProductId = 2, Name = "Category Product 2", CategoryId = 1 }
        };
        _backendServiceMock.Setup(x => x.GetProductsByCategoryAsync(1))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsByCategory(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Equal(2, returnedProducts.Count());
        Assert.All(returnedProducts, product => Assert.Equal(1, product.CategoryId));
    }

    [Fact]
    public async Task GetProductsByCategory_WhenBackendThrows_Returns500()
    {
        // Arrange
        _backendServiceMock.Setup(x => x.GetProductsByCategoryAsync(1))
            .ThrowsAsync(new BackendServiceException("Test error"));

        // Act
        var result = await _controller.GetProductsByCategory(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var statusCodeResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Internal server error", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetProductsByCategory_WithEmptyCategory_ReturnsOkWithEmptyList()
    {
        // Arrange
        _backendServiceMock.Setup(x => x.GetProductsByCategoryAsync(999))
            .ReturnsAsync(Enumerable.Empty<Product>());

        // Act
        var result = await _controller.GetProductsByCategory(999);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);
        Assert.Empty(returnedProducts);
    }
}