using APIGateway.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace APIGateway.Services;

public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BackendService> _logger;
    private readonly IMemoryCache _cache;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public BackendService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BackendService> logger,
        IMemoryCache cache,
        IWebHostEnvironment env)
    {
        _httpClient = httpClient;
        
        if (env.IsDevelopment() && _httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(configuration["BackendApi:BaseUrl"] ?? "https://localhost:7242");
        }

        _logger = logger;
        _cache = cache;

        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        outcome.Exception,
                        "Retrying request after {RetryCount} attempts. Delay: {Delay}ms",
                        retryCount,
                        timeSpan.TotalMilliseconds);
                });
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        const string cacheKey = "all_products";

        if (_cache.TryGetValue<IEnumerable<Product>>(cacheKey, out var cachedProducts))
        {
            _logger.LogInformation("Returning products from cache");
            return cachedProducts!;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync("/api/Product"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<Product>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? 
                Enumerable.Empty<Product>();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, products, cacheOptions);

            return products;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error connecting to backend service");
            throw new BackendServiceException("Unable to connect to the backend service", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching products from backend");
            throw new BackendServiceException("An unexpected error occurred while fetching products", ex);
        }
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        string cacheKey = $"product_{id}";

        if (_cache.TryGetValue<Product>(cacheKey, out var cachedProduct))
        {
            _logger.LogInformation("Returning product {Id} from cache", id);
            return cachedProduct;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/api/Product/{id}"));

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<Product>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                _cache.Set(cacheKey, product, cacheOptions);
            }

            return product;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error connecting to backend service for product {Id}", id);
            throw new BackendServiceException($"Unable to connect to the backend service while fetching product {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching product {Id} from backend", id);
            throw new BackendServiceException($"An unexpected error occurred while fetching product {id}", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        string cacheKey = $"category_{categoryId}_products";

        if (_cache.TryGetValue<IEnumerable<Product>>(cacheKey, out var cachedProducts))
        {
            _logger.LogInformation("Returning products for category {CategoryId} from cache", categoryId);
            return cachedProducts!;
        }

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/api/Product/category/{categoryId}"));

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<IEnumerable<Product>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? 
                Enumerable.Empty<Product>();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _cache.Set(cacheKey, products, cacheOptions);

            return products;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error connecting to backend service for category {CategoryId}", categoryId);
            throw new BackendServiceException($"Unable to connect to the backend service while fetching products for category {categoryId}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching products for category {CategoryId} from backend", categoryId);
            throw new BackendServiceException($"An unexpected error occurred while fetching products for category {categoryId}", ex);
        }
    }
}

public class BackendServiceException : Exception
{
    public BackendServiceException(string message) : base(message) { }
    public BackendServiceException(string message, Exception innerException) : base(message, innerException) { }
}
