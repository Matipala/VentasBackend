namespace VentasBackend.Application.DTOs;

public class CuentaTicketItemResponse
{
    public int IdCuentaTicketItem { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Nota { get; set; }
    public string EstadoComanda { get; set; } = "NUEVO";
}
