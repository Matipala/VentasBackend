using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("clientes", Schema = "ventas")]
public class Cliente
{
    [Key]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("id_empresa")]
    public int IdEmpresa { get; set; }

    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("telefono")]
    [MaxLength(30)]
    public string Telefono { get; set; } = string.Empty;
}
