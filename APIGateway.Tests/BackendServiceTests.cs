using APIGateway.Models;
using APIGateway.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace APIGateway.Tests;

public class BackendServiceTests
{
    private readonly Mock<ILogger<BackendService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly IMemoryCache _cache;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;

    public BackendServiceTests()
    {
        _loggerMock = new Mock<ILogger<BackendService>>();
        _configurationMock = new Mock<IConfiguration>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _envMock = new Mock<IWebHostEnvironment>();
        _handlerMock = new Mock<HttpMessageHandler>();
        
        _configurationMock
            .Setup(x => x["BackendApi:BaseUrl"])
            .Returns("https://localhost:7242");

        _envMock
            .Setup(x => x.EnvironmentName)
            .Returns("Development");
    }

    [Fact]
    public async Task GetProductsAsync_WhenCacheEmpty_CallsBackendAndCachesResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Test Product 1" },
            new Product { ProductId = 2, Name = "Test Product 2" }
        };

        SetupMockHttpResponse("/api/Product", products);

        var service = CreateBackendService();

        // Act
        var result = await service.GetProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        VerifyHttpCall("/api/Product", Times.Once());
    }

    [Fact]
    public async Task GetProductsAsync_WhenCached_ReturnsCachedResult()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Test Product 1" }
        };

        SetupMockHttpResponse("/api/Product", products);

        var service = CreateBackendService();

        // Act
        await service.GetProductsAsync(); // First call
        var result = await service.GetProductsAsync(); // Second call

        // Assert
        Assert.Single(result);
        VerifyHttpCall("/api/Product", Times.Once()); // Should only call once due to caching
    }

    [Fact]
    public async Task GetProductsAsync_WhenBackendFails_ThrowsBackendServiceException()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product", HttpStatusCode.InternalServerError);
        var service = CreateBackendService();

        // Act & Assert
        await Assert.ThrowsAsync<BackendServiceException>(() => service.GetProductsAsync());
        VerifyHttpCall("/api/Product", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product { ProductId = 1, Name = "Test Product" };
        SetupMockHttpResponse("/api/Product/1", product);
        var service = CreateBackendService();

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        VerifyHttpCall("/api/Product/1", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductNotFound_ReturnsNull()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product/999", HttpStatusCode.NotFound);
        var service = CreateBackendService();

        // Act
        var result = await service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
        VerifyHttpCall("/api/Product/999", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenCached_ReturnsCachedProduct()
    {
        // Arrange
        var product = new Product { ProductId = 1, Name = "Test Product" };
        SetupMockHttpResponse("/api/Product/1", product);
        var service = CreateBackendService();

        // Act
        await service.GetProductByIdAsync(1); // First call
        var result = await service.GetProductByIdAsync(1); // Second call

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        VerifyHttpCall("/api/Product/1", Times.Once()); // Should only call once due to caching
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsProductsForCategory()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Category Product 1", CategoryId = 1 },
            new Product { ProductId = 2, Name = "Category Product 2", CategoryId = 1 }
        };
        SetupMockHttpResponse("/api/Product/category/1", products);
        var service = CreateBackendService();

        // Act
        var result = await service.GetProductsByCategoryAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, product => Assert.Equal(1, product.CategoryId));
        VerifyHttpCall("/api/Product/category/1", Times.Once());
    }

    [Fact]
    public async Task GetProductsByCategory_WhenCached_ReturnsCachedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Category Product 1", CategoryId = 1 }
        };
        SetupMockHttpResponse("/api/Product/category/1", products);
        var service = CreateBackendService();

        // Act
        await service.GetProductsByCategoryAsync(1); // First call
        var result = await service.GetProductsByCategoryAsync(1); // Second call

        // Assert
        Assert.Single(result);
        VerifyHttpCall("/api/Product/category/1", Times.Once()); // Should only call once due to caching
    }

    [Fact]
    public async Task GetProductsByCategory_WhenBackendFails_ThrowsBackendServiceException()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product/category/1", HttpStatusCode.InternalServerError);
        var service = CreateBackendService();

        // Act & Assert
        await Assert.ThrowsAsync<BackendServiceException>(() => service.GetProductsByCategoryAsync(1));
        VerifyHttpCall("/api/Product/category/1", Times.Once());
    }

    private BackendService CreateBackendService()
    {
        var client = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:7242")
        };

        return new BackendService(
            client,
            _configurationMock.Object,
            _loggerMock.Object,
            _cache,
            _envMock.Object);
    }

    private void SetupMockHttpResponse<T>(string url, T response)
    {
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(response))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.PathAndQuery == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    private void SetupMockHttpResponseWithStatusCode(string url, HttpStatusCode statusCode)
    {
        var responseMessage = new HttpResponseMessage(statusCode);

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.PathAndQuery == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    private void VerifyHttpCall(string url, Times times)
    {
        _handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.PathAndQuery == url),
                ItExpr.IsAny<CancellationToken>());
    }
}