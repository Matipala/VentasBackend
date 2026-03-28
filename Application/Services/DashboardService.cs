using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Data;

namespace VentasBackend.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly VentasDbContext _context;

        public DashboardService(VentasDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetResumenDiarioAsync(int empresaId)
        {
            var hoy = DateTime.UtcNow.Date;

            var pagosHoy = await _context.Pagos
                .Where(p => p.IdEmpresa == empresaId && p.FechaPago >= hoy)
                .ToListAsync();

            var totalVentas = pagosHoy.Sum(p => p.Monto);
            var cantidadTickets = pagosHoy.Count;
            var promedioTicket = cantidadTickets > 0 ? totalVentas / cantidadTickets : 0;

            return new
            {
                totalVentas,
                cantidadTickets,
                promedioTicket
            };
        }

        public async Task<IEnumerable<object>> GetTopProductosAsync(int empresaId)
        {
            var hoy = DateTime.UtcNow.Date;

            return await _context.CuentasTicketItems
                .Join(_context.CuentasTickets, 
                      i => i.IdCuentaTicket, 
                      c => c.IdCuentaTicket, 
                      (i, c) => new { Item = i, Cuenta = c })
                .Where(x => x.Cuenta.IdEmpresa == empresaId && x.Cuenta.Estado == "PAGADO" && x.Cuenta.FechaPago >= hoy)
                .GroupBy(x => x.Item.IdProducto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    Cantidad = g.Sum(x => x.Item.Cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetCargaKdsAsync(int empresaId)
        {
            return await _context.CuentasTicketItems
                .Join(_context.CuentasTickets, i => i.IdCuentaTicket, c => c.IdCuentaTicket, (i, c) => new { Item = i, Cuenta = c })
                .Where(x => x.Cuenta.IdEmpresa == empresaId && x.Cuenta.Estado == "ABIERTO")
                .GroupBy(x => x.Item.EstadoComanda)
                .Select(g => new
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync();
        }
    }
}
