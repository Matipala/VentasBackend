namespace VentasBackend.Application.DTOs;

public class CuentaTicketItemResponse
{
    public Guid IdCuentaTicketItem { get; set; }
    public Guid IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string? Nota { get; set; }
    public string EstadoComanda { get; set; } = "NUEVO";
}
