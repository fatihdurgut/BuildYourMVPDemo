using Frontend.Models;
using System.Text.Json;

namespace Frontend.Services;

public class ProductService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<ProductService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public ProductService(
        IHttpClientFactory clientFactory,
        ILogger<ProductService> logger,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _configuration = configuration;
        _env = env;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync("/api/Product");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<IEnumerable<Product>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                return products ?? Enumerable.Empty<Product>();
            }
            
            _logger.LogWarning("Product API returned status code: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from API Gateway");
            throw;
        }
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/Product/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Product>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            
            _logger.LogWarning("Product API returned status code: {StatusCode} for product {Id}", 
                response.StatusCode, id);
            throw new HttpRequestException($"Error fetching product {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {Id} from API Gateway", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync($"/api/Product/category/{categoryId}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<IEnumerable<Product>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                return products ?? Enumerable.Empty<Product>();
            }
            
            _logger.LogWarning("Product API returned status code: {StatusCode} for category {CategoryId}", 
                response.StatusCode, categoryId);
            return Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products for category {CategoryId} from API Gateway", 
                categoryId);
            throw;
        }
    }

    private HttpClient GetClient()
    {
        var client = _clientFactory.CreateClient("ApiGateway");
            
        if (_env.IsDevelopment() && client.BaseAddress == null)
        {
            // In development, if no base address is set, configure it
            client.BaseAddress = new Uri(_configuration["ApiGateway:BaseUrl"] ?? "https://localhost:7108");
        }

        return client;
    }
}