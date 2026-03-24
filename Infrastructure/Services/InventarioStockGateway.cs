using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VentasBackend.Application.Interface;

namespace VentasBackend.Infrastructure.Services
{
    public class InventarioStockGateway : IStockGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public InventarioStockGateway(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["InventoryApi:BaseUrl"] ?? throw new ArgumentNullException("InventoryApi:BaseUrl");
        }

        public async Task<bool> ValidarStockAsync(int productoId, int almacenId, decimal cantidad)
        {
            var url = $"{_baseUrl}/api/stock/validar";
            var response = await _httpClient.PostAsJsonAsync(url, new { productoId, almacenId, cantidad });
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }

        public async Task<bool> DescontarStockAsync(int productoId, int almacenId, decimal cantidad)
        {
            var url = $"{_baseUrl}/api/stock/descontar";
            var response = await _httpClient.PostAsJsonAsync(url, new { productoId, almacenId, cantidad });
            if (!response.IsSuccessStatusCode) return false;
            var result = await response.Content.ReadFromJsonAsync<bool>();
            return result;
        }

        public async Task<decimal> ConsultarStockActualAsync(int productoId, int almacenId)
        {
            var url = $"{_baseUrl}/api/stock/actual?productoId={productoId}&almacenId={almacenId}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return 0;
            var result = await response.Content.ReadFromJsonAsync<decimal>();
            return result;
        }
    }
}
