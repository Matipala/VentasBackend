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

        public async Task<bool> ValidarStockAsync(int productoId, int almacenId, decimal cantidad, int idEmpresa)
        {
            var url = $"{_baseUrl}/api/stock/validar";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-empresa-id", idEmpresa.ToString());
            request.Content = JsonContent.Create(new { ProductoId = productoId, AlmacenId = almacenId, Cantidad = cantidad });
            
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return false;
            
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> DescontarStockAsync(int productoId, int almacenId, decimal cantidad, int idEmpresa)
        {
            var url = $"{_baseUrl}/api/stock/descontar";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-empresa-id", idEmpresa.ToString());
            request.Content = JsonContent.Create(new { ProductoId = productoId, AlmacenId = almacenId, Cantidad = cantidad });
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<decimal> ConsultarStockActualAsync(int productoId, int almacenId, int idEmpresa)
        {
            var url = $"{_baseUrl}/api/stock/actual?productoId={productoId}&almacenId={almacenId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-empresa-id", idEmpresa.ToString());
            
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return 0;
            
            return await response.Content.ReadFromJsonAsync<decimal>();
        }
    }
}
