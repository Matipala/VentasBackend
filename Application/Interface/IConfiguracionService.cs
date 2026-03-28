using System.Threading.Tasks;
using VentasBackend.Domain.Entities;

namespace VentasBackend.Application.Interface
{
    public interface IConfiguracionService
    {
        Task<ConfiguracionVentas> ObtenerConfiguracionAsync(int empresaId);
        Task<ConfiguracionVentas> ActualizarConfiguracionAsync(int empresaId, ConfiguracionVentas request);
    }
}
