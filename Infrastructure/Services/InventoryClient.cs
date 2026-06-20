using System.Net.Http.Json;
using Polly;
using Polly.CircuitBreaker;
using VentasBackend.Application.Interface;

namespace VentasBackend.Infrastructure.Services;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly IAsyncPolicy _circuitBreaker;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"[Polly Retry] Intento {retryCount}/3 tras {timeSpan.TotalSeconds}s - {exception.GetType().Name}");
                });

        _circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, ts) => Console.WriteLine($"[Polly CircuitBreaker] ABIERTO por {ts.TotalSeconds}s - {ex.Message}"),
                onReset: () => Console.WriteLine("[Polly CircuitBreaker] CERRADO - circuito restaurado"),
                onHalfOpen: () => Console.WriteLine("[Polly CircuitBreaker] SEMI-ABIERTO - probando..."));
    }

    public async Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items)
    {
        var resilience = Policy.WrapAsync(_retryPolicy, _circuitBreaker);

        try
        {
            return await resilience.ExecuteAsync(async () =>
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
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Polly Fallback] ValidateStock - Inventario no disponible: {ex.GetType().Name}");
            return new StockValidationResponseDto
            {
                IsValid = false,
                Requirements = new List<StockRequirementDto>
                {
                    new StockRequirementDto
                    {
                        ProductCen = "SISTEMA",
                        ProductName = "Inventario",
                        Reason = "Inventario no disponible temporalmente. Intente nuevamente en unos segundos.",
                        RequestedQuantity = 0,
                        AvailableQuantity = 0,
                        MissingQuantity = 0
                    }
                }
            };
        }
    }

    public async Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items)
    {
        var resilience = Policy.WrapAsync(_retryPolicy, _circuitBreaker);

        try
        {
            return await resilience.ExecuteAsync(async () =>
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
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Polly Fallback] ConsumeStock - Inventario no disponible: {ex.GetType().Name}");
            return false;
        }
    }

    public async Task<List<SellableProductContractResponseDto>> GetSellableProductsAsync(
        string companyCen, string? search, string? categoryCen, string? warehouseCen, bool onlyAvailable)
    {
        var resilience = Policy.WrapAsync(_retryPolicy, _circuitBreaker);

        try
        {
            return await resilience.ExecuteAsync(async () =>
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
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Polly Fallback] GetSellableProducts - Inventario no disponible: {ex.GetType().Name}");
            return new List<SellableProductContractResponseDto>();
        }
    }

    public async Task<Dictionary<string, string>> LookupProductNamesAsync(string companyCen, List<string> productCens)
    {
        if (!productCens.Any())
            return new Dictionary<string, string>();

        var resilience = Policy.WrapAsync(_retryPolicy, _circuitBreaker);

        try
        {
            return await resilience.ExecuteAsync(async () =>
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
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Polly Fallback] LookupProductNames - Inventario no disponible: {ex.GetType().Name}");
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
