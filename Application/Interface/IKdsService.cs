using System.Collections.Generic;
using System.Threading.Tasks;

namespace VentasBackend.Application.Interface
{
    public interface IKdsService
    {
        Task<IEnumerable<object>> GetItemsPendientesAsync(Guid empresaId, string? estacion = null);
    }
}
