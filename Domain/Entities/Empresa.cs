using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VentasBackend.Domain.Entities;

[Table("Empresas", Schema = "shared")]
public class Empresa
{
    [Key]
    [Column("id_empresa")]
    public Guid IdEmpresa { get; set; } = Guid.NewGuid();

    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("activo")]
    public bool Activo { get; set; } = true;
}
