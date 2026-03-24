using System.Threading.Tasks;

namespace VentasBackend.Business.Interface
{
    public interface IStockGateway
    {
        Task<bool> ValidarStockAsync(int productoId, int almacenId, decimal cantidad);
        Task<bool> DescontarStockAsync(int productoId, int almacenId, decimal cantidad);
        Task<decimal> ConsultarStockActualAsync(int productoId, int almacenId);
    }
}
