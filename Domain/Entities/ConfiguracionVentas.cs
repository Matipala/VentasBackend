using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("ConfiguracionVentas", Schema = "ventas")]
public class ConfiguracionVentas
{
    [Key]
    [Column("id_configuracion")]
    public int IdConfiguracion { get; set; }

    [Column("id_empresa")]
    public int IdEmpresa { get; set; }

    [Column("nombre_impuesto")]
    [MaxLength(50)]
    public string NombreImpuesto { get; set; } = "IVA";

    [Column("porcentaje_impuesto")]
    public decimal PorcentajeImpuesto { get; set; } = 0m;
}
