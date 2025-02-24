using Microsoft.AspNetCore.Mvc.RazorPages;
using Frontend.Services;
using Frontend.Models;

namespace Frontend.Pages;



public class IndexModel : PageModel
{
    private readonly ProductService _productService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Product> Products { get; private set; } = Enumerable.Empty<Product>();
    public IDictionary<int, IEnumerable<Product>> ProductsByCategory { get; private set; } = new Dictionary<int, IEnumerable<Product>>();
    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public IndexModel(ProductService productService, ILogger<IndexModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Products = await _productService.GetProductsAsync();
            
            // Group products by category
            ProductsByCategory = Products
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            ErrorMessage = "Unable to load products at this time. Please try again later.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
