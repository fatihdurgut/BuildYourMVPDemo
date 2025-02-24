using Frontend.Models;
using Frontend.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Frontend.Tests;

public class ProductServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public ProductServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _configMock = new Mock<IConfiguration>();
        _envMock = new Mock<IWebHostEnvironment>();
        _handlerMock = new Mock<HttpMessageHandler>();
        
        // Setup mock handler
        _httpClient = new HttpClient(_handlerMock.Object) 
        {
            BaseAddress = new Uri("https://localhost:7108")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("ApiGateway"))
            .Returns(_httpClient);

        _configMock
            .Setup(x => x["ApiGateway:BaseUrl"])
            .Returns("https://localhost:7108");

        _envMock
            .SetupGet(x => x.EnvironmentName)
            .Returns(Environments.Development);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsProducts_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new Product { ProductId = 1, Name = "Test Product 1", Price = 10.99m },
            new Product { ProductId = 2, Name = "Test Product 2", Price = 20.99m }
        };

        SetupMockHttpResponse("/api/Product", HttpStatusCode.OK, expectedProducts);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("Test Product 1", result.First().Name);
        VerifyHttpCall("/api/Product", Times.Once());
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsEmptyList_WhenApiCallFails()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product", HttpStatusCode.InternalServerError);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductsAsync();

        // Assert
        Assert.Empty(result);
        VerifyHttpCall("/api/Product", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        var expectedProduct = new Product { ProductId = 1, Name = "Test Product", Price = 10.99m };
        
        SetupMockHttpResponse("/api/Product/1", HttpStatusCode.OK, expectedProduct);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.Name);
        VerifyHttpCall("/api/Product/1", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsNull_WhenProductNotFound()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product/999", HttpStatusCode.NotFound);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductByIdAsync(999);

        // Assert
        Assert.Null(result);
        VerifyHttpCall("/api/Product/999", Times.Once());
    }

    [Fact]
    public async Task GetProductByIdAsync_ThrowsException_WhenApiReturnsError()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product/1", HttpStatusCode.InternalServerError);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetProductByIdAsync(1));
        VerifyHttpCall("/api/Product/1", Times.Once());
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsProducts_WhenCategoryExists()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new Product { ProductId = 1, Name = "Category Product 1", CategoryId = 1 },
            new Product { ProductId = 2, Name = "Category Product 2", CategoryId = 1 }
        };

        SetupMockHttpResponse("/api/Product/category/1", HttpStatusCode.OK, expectedProducts);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductsByCategoryAsync(1);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, product => Assert.Equal(1, product.CategoryId));
        VerifyHttpCall("/api/Product/category/1", Times.Once());
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsEmptyList_WhenCategoryNotFound()
    {
        // Arrange
        SetupMockHttpResponseWithStatusCode("/api/Product/category/999", HttpStatusCode.NotFound);

        var service = new ProductService(
            _httpClientFactoryMock.Object,
            _loggerMock.Object,
            _configMock.Object,
            _envMock.Object);

        // Act
        var result = await service.GetProductsByCategoryAsync(999);

        // Assert
        Assert.Empty(result);
        VerifyHttpCall("/api/Product/category/999", Times.Once());
    }

    private void SetupMockHttpResponse<T>(string url, HttpStatusCode statusCode, T content)
    {
        var response = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            response.Content = new StringContent(
                JsonSerializer.Serialize(content),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.PathAndQuery == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void SetupMockHttpResponseWithStatusCode(string url, HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode);

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri!.PathAndQuery == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
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