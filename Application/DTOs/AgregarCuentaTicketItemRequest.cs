using System.Text.Json.Serialization;

namespace VentasBackend.Application.DTOs;

public class AgregarCuentaTicketItemRequest
{
    [JsonPropertyName("idProducto")]
    public int IdProducto { get; set; }

    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; }

    [JsonPropertyName("precioUnitario")]
    public decimal? PrecioUnitario { get; set; }

    [JsonPropertyName("nota")]
    public string? Nota { get; set; }
}