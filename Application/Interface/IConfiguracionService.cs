using System.Threading.Tasks;
using VentasBackend.Domain.Entities;

namespace VentasBackend.Application.Interface
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionVentas> ObtenerConfiguracionAsync(Guid empresaId);
        Task<ConfiguracionVentas> ActualizarConfiguracionAsync(Guid empresaId, ConfiguracionVentas request);
    }
}
