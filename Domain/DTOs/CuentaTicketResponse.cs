namespace VentasBackend.Domain.DTOs;

public class CuentaTicketResponse
{
    public int IdCuentaTicket { get; set; }
    public int Numero { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IdAlmacen { get; set; }
    public string Mesero { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaPago { get; set; }
}