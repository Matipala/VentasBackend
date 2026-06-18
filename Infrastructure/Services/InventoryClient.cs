using System.Net.Http.Json;
using VentasBackend.Application.Interface;
using Microsoft.Extensions.Configuration;

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
}
