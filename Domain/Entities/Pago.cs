using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("pagos", Schema = "ventas")]
public class Pago
{
    [Key]
    [Column("id_pago")]
    public int IdPago { get; set; }

    [Column("id_empresa")]
    public int IdEmpresa { get; set; }

    [Column("id_cuenta_ticket")]
    public int IdCuentaTicket { get; set; }

    [Column("metodo_pago")]
    [MaxLength(20)]
    public string MetodoPago { get; set; } = string.Empty;

    [Column("monto")]
    public decimal Monto { get; set; }

    [Column("fecha_pago")]
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
}
