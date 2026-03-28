namespace VentasBackend.Application.DTOs;

public class CrearCuentaTicketRequest
{
    public int IdAlmacen { get; set; }
    public int? IdCliente { get; set; }
    public string Mesero { get; set; } = string.Empty;
}