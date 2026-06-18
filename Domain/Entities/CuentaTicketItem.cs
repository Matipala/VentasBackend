using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("cuentas_ticket_items", Schema = "ventas")]
public class CuentaTicketItem
{
    [Key]
    [Column("id_cuenta_ticket_item")]
    public Guid IdCuentaTicketItem { get; set; } = Guid.NewGuid();

    [Column("id_cuenta_ticket")]
    public Guid IdCuentaTicket { get; set; }

    [Column("id_producto")]
    public Guid IdProducto { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    public decimal PrecioUnitario { get; set; }

    [Column("subtotal")]
    public decimal Subtotal { get; set; }

    [Column("nota")]
    public string? Nota { get; set; }

    [Column("estado_comanda")]
    [MaxLength(20)]
    public string EstadoComanda { get; set; } = "NUEVO";
}
