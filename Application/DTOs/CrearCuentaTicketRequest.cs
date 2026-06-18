namespace VentasBackend.Application.DTOs;

public class CrearCuentaTicketRequest
{
    public Guid IdAlmacen { get; set; }
    public Guid? IdCliente { get; set; }
    public string Mesero { get; set; } = string.Empty;
}
