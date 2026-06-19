using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Data;

namespace VentasBackend.Application.Services
{
    public class KdsService : IKdsService
    {
        private readonly VentasDbContext _context;

        public KdsService(VentasDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetItemsPendientesAsync(Guid empresaId, string? estacion = null)
        {
            var query = from item in _context.CuentasTicketItems
                        join cuenta in _context.CuentasTickets on item.IdCuentaTicket equals cuenta.IdCuentaTicket
                        where cuenta.IdEmpresa == empresaId
                              && cuenta.Estado == "ABIERTO"
                              && item.EstadoComanda != "NUEVO"
                        orderby cuenta.FechaCreacion ascending
                        select new
                        {
                            item.IdCuentaTicketItem,
                            item.IdCuentaTicket,
                            cuenta.Numero,
                            cuenta.Mesero,
                            item.IdProducto,
                            item.Cantidad,
                            item.Nota,
                            EstadoComanda = (item.EstadoComanda ?? "NUEVO").ToUpperInvariant(),
                            cuenta.FechaCreacion
                        };

            return await query.ToListAsync();
        }
    }
}
