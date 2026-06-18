using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("pagos", Schema = "ventas")]
public class Pago
{
    [Key]
    [Column("id_pago")]
    public Guid IdPago { get; set; } = Guid.NewGuid();

    [Column("id_empresa")]
    public Guid IdEmpresa { get; set; }

    [Column("id_cuenta_ticket")]
    public Guid IdCuentaTicket { get; set; }

    [Column("metodo_pago")]
    [MaxLength(20)]
    public string MetodoPago { get; set; } = string.Empty;

    [Column("monto")]
    public decimal Monto { get; set; }

    [Column("fecha_pago")]
    public DateTime FechaPago { get; set; } = DateTime.UtcNow;
}
