using Microsoft.EntityFrameworkCore;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Data;

namespace VentasBackend.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly VentasDbContext _context;
    private readonly IInventoryClient _inventoryClient;

    public DashboardService(VentasDbContext context, IInventoryClient inventoryClient)
    {
        _context = context;
        _inventoryClient = inventoryClient;
    }

    public async Task<object> GetResumenDiarioAsync(Guid empresaId)
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
            totalSales = totalVentas,
            ticketsCount = cantidadTickets,
            averageTicket = promedioTicket
        };
    }

    public async Task<IEnumerable<object>> GetTopProductosAsync(Guid empresaId)
    {
        var hoy = DateTime.UtcNow.Date;
        var companyCen = empresaId.ToString();

        var topItems = await _context.CuentasTicketItems
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

        var productCens = topItems.Select(t => t.IdProducto.ToString()).ToList();
        Dictionary<string, string> productNames;
        try
        {
            productNames = await _inventoryClient.LookupProductNamesAsync(companyCen, productCens);
        }
        catch
        {
            productNames = new Dictionary<string, string>();
        }

        return topItems.Select(t => new
        {
            productCen = t.IdProducto.ToString(),
            productName = productNames.GetValueOrDefault(t.IdProducto.ToString(), $"Producto {t.IdProducto}"),
            totalQuantity = t.Cantidad
        });
    }

    public async Task<IEnumerable<object>> GetCargaKdsAsync(Guid empresaId)
    {
        return await _context.CuentasTicketItems
            .Join(_context.CuentasTickets, i => i.IdCuentaTicket, c => c.IdCuentaTicket, (i, c) => new { Item = i, Cuenta = c })
            .Where(x => x.Cuenta.IdEmpresa == empresaId && x.Cuenta.Estado == "ABIERTO")
            .GroupBy(x => x.Item.EstadoComanda)
            .Select(g => new
            {
                estado = g.Key,
                cantidad = g.Count()
            })
            .ToListAsync();
    }
}
