using System.Net.Http.Json;
using VentasBackend.Application.Interface;

namespace VentasBackend.Infrastructure.Services;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items)
    {
        var request = new
        {
            WarehouseCen = warehouseCen,
            Source = "SALES_POS",
            ReferenceCen = Guid.NewGuid().ToString(),
            Items = items.Select(i => new { ProductCen = i.ProductCen, Quantity = i.Quantity }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync($"/api/inventory/companies/{companyCen}/stock/validate", request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Inventory API Error: {response.StatusCode} - {errorBody}");
            return new StockValidationResponseDto { IsValid = false };
        }

        var result = await response.Content.ReadFromJsonAsync<StockValidationResponseDto>();
        return result ?? new StockValidationResponseDto { IsValid = false };
    }

    public async Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items)
    {
        var request = new
        {
            WarehouseCen = warehouseCen,
            Source = "SALES_POS",
            ReferenceCen = referenceCen,
            Reason = reason,
            Items = items.Select(i => new { ProductCen = i.ProductCen, Quantity = i.Quantity }).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync($"/api/inventory/companies/{companyCen}/stock/consume", request);

        return response.IsSuccessStatusCode;
    }

    public async Task<List<SellableProductContractResponseDto>> GetSellableProductsAsync(
        string companyCen, string? search, string? categoryCen, string? warehouseCen, bool onlyAvailable)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search)) queryParams.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrEmpty(categoryCen)) queryParams.Add($"categoryCen={Uri.EscapeDataString(categoryCen)}");
            if (!string.IsNullOrEmpty(warehouseCen)) queryParams.Add($"warehouseCen={Uri.EscapeDataString(warehouseCen)}");
            if (onlyAvailable) queryParams.Add("onlyAvailable=true");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/api/inventory/companies/{companyCen}/sellable-products{query}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SellableProductContractResponseDto>>();
                return result ?? new List<SellableProductContractResponseDto>();
            }

            return new List<SellableProductContractResponseDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sellable products: {ex.Message}");
            return new List<SellableProductContractResponseDto>();
        }
    }

    public async Task<Dictionary<string, string>> LookupProductNamesAsync(string companyCen, List<string> productCens)
    {
        if (!productCens.Any())
            return new Dictionary<string, string>();

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/inventory/companies/{companyCen}/products/lookup",
                new { productCens });

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadFromJsonAsync<List<ProductLookupItemDto>>();
                if (results != null)
                    return results.ToDictionary(r => r.ProductCen, r => r.Name);
            }

            return new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error looking up product names: {ex.Message}");
            return new Dictionary<string, string>();
        }
    }

    private class ProductLookupItemDto
    {
        public string ProductCen { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
