using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentasBackend.Application.Interface
{
    public interface IDashboardService
    {
        Task<object> GetResumenDiarioAsync(Guid empresaId);
        Task<IEnumerable<object>> GetTopProductosAsync(Guid empresaId);
        Task<IEnumerable<object>> GetCargaKdsAsync(Guid empresaId);
    }
}
