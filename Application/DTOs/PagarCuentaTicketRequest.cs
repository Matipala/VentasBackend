using System.Text.Json.Serialization;

namespace VentasBackend.Application.DTOs;

public class PagarCuentaTicketRequest
{
    [JsonPropertyName("metodoPago")]
    public string MetodoPago { get; set; } = string.Empty;
}