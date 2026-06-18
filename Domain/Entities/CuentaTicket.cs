using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("cuentas_tickets", Schema = "ventas")]
public class CuentaTicket
{
    [Key]
    [Column("id_cuenta_ticket")]
    public Guid IdCuentaTicket { get; set; } = Guid.NewGuid();

    [Column("id_empresa")]
    public Guid IdEmpresa { get; set; }

    [Column("id_almacen")]
    public Guid IdAlmacen { get; set; }

    [Column("id_cliente")]
    public Guid? IdCliente { get; set; }

    [Column("numero")]
    public int Numero { get; set; }

    [Column("mesero")]
    [MaxLength(100)]
    public string Mesero { get; set; } = string.Empty;

    [Column("estado")]
    [MaxLength(30)]
    public string Estado { get; set; } = "ABIERTO";

    [Column("subtotal")]
    public decimal Subtotal { get; set; } = 0m;

    [Column("impuesto")]
    public decimal Impuesto { get; set; } = 0m;

    [Column("total")]
    public decimal Total { get; set; } = 0m;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("fecha_pago")]
    public DateTime? FechaPago { get; set; }
}
