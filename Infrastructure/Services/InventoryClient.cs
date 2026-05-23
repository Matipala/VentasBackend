using System.Net.Http.Json;
using VentasBackend.Application.Interface;
using Microsoft.Extensions.Configuration;

namespace VentasBackend.Infrastructure.Services;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public InventoryClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["InventoryApi:BaseUrl"];
    }

    public async Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items)
    {
        var request = new
        {
            warehouseCen = warehouseCen,
            source = "SALES_PAYMENT",
            items = items
        };

        var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/validate";
        var response = await _httpClient.PostAsJsonAsync(url, request);

        if (!response.IsSuccessStatusCode)
            return new StockValidationResponseDto { IsValid = false };

        return await response.Content.ReadFromJsonAsync<StockValidationResponseDto>() ?? new StockValidationResponseDto { IsValid = false };
    }

    public async Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items)
    {
        var request = new
        {
            warehouseCen = warehouseCen,
            source = "SALES_PAYMENT",
            referenceCen = referenceCen,
            reason = reason,
            items = items
        };

        var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/consume";
        var response = await _httpClient.PostAsJsonAsync(url, request);

        return response.IsSuccessStatusCode;
    }
}
