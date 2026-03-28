using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentasBackend.Application.Interface
{
    public interface IDashboardService
    {
        Task<object> GetResumenDiarioAsync(int empresaId);
        Task<IEnumerable<object>> GetTopProductosAsync(int empresaId);
        Task<IEnumerable<object>> GetCargaKdsAsync(int empresaId);
    }
}
