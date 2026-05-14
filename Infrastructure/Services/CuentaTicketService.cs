using Microsoft.EntityFrameworkCore;
using VentasBackend.Application.DTOs;
using VentasBackend.Application.Interface;
using VentasBackend.Domain.Entities;
using VentasBackend.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using VentasBackend.Presentation.Hubs;

namespace VentasBackend.Application.Services;

public class CuentaTicketService : ICuentaTicketService
{
    private readonly VentasDbContext _context;
    private readonly IHubContext<KdsHub> _hubContext;

    public CuentaTicketService(VentasDbContext context, IHubContext<KdsHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CrearCuentaAsync(CrearCuentaTicketRequest request, int idEmpresa)
    {
        var ultimoNumero = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == idEmpresa)
            .OrderByDescending(c => c.Numero)
            .Select(c => c.Numero)
            .FirstOrDefaultAsync();

        var nuevaCuenta = new CuentaTicket
        {
            IdEmpresa = idEmpresa,
            IdAlmacen = request.IdAlmacen,
            IdCliente = request.IdCliente,
            Mesero = request.Mesero,
            Numero = ultimoNumero + 1,
            Estado = "ABIERTO",
            FechaCreacion = DateTime.UtcNow
        };

        _context.CuentasTickets.Add(nuevaCuenta);
        await _context.SaveChangesAsync();

        return (true, "Cuenta creada exitosamente", await ObtenerCuentaAsync(nuevaCuenta.IdCuentaTicket, idEmpresa));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> AgregarItemAsync(int idCuentaTicket, AgregarCuentaTicketItemRequest request, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return (false, "Cuenta no encontrada", null);
        if (cuenta.Estado != "ABIERTO") return (false, "La cuenta ya no está abierta", null);

        var precio = request.PrecioUnitario ?? 0m; 

        var itemExistente = await _context.CuentasTicketItems
            .FirstOrDefaultAsync(i => i.IdCuentaTicket == idCuentaTicket 
                && i.IdProducto == request.IdProducto 
                && (i.EstadoComanda == "NUEVO" || string.IsNullOrEmpty(i.EstadoComanda)));

        if (itemExistente != null)
        {
            itemExistente.Cantidad += request.Cantidad;
            itemExistente.Subtotal += precio * request.Cantidad;
            
            if (!string.IsNullOrWhiteSpace(request.Nota))
            {
                string notaFormateada = $"x{request.Cantidad} {request.Nota.Trim()}";
                if (string.IsNullOrWhiteSpace(itemExistente.Nota))
                    itemExistente.Nota = notaFormateada;
                else
                    itemExistente.Nota += " | " + notaFormateada;
            }
            
            cuenta.Subtotal += precio * request.Cantidad;
        }
        else
        {
            var nuevoItem = new CuentaTicketItem
            {
                IdCuentaTicket = idCuentaTicket,
                IdProducto = request.IdProducto,
                Cantidad = request.Cantidad,
                PrecioUnitario = precio,
                Subtotal = precio * request.Cantidad,
                Nota = !string.IsNullOrWhiteSpace(request.Nota) ? $"x{request.Cantidad} {request.Nota.Trim()}" : null,
                EstadoComanda = "NUEVO"
            };

            _context.CuentasTicketItems.Add(nuevoItem);
            cuenta.Subtotal += nuevoItem.Subtotal;
        }
        
        var config = await _context.Configuracion.FirstOrDefaultAsync(c => c.IdEmpresa == idEmpresa);
        var pctImpuesto = config?.PorcentajeImpuesto ?? 0m;
        
        cuenta.Impuesto = cuenta.Subtotal * (pctImpuesto / 100m);
        cuenta.Total = cuenta.Subtotal + cuenta.Impuesto;

        await _context.SaveChangesAsync();

        return (true, "Item agregado exitosamente", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(int idCuentaTicket, PagarCuentaTicketRequest request, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return (false, "Cuenta no encontrada", null);
        if (cuenta.Estado != "ABIERTO") return (false, "La cuenta ya está pagada o cancelada", null);

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        if (!items.Any()) return (false, "La cuenta no tiene items para pagar", null);

        var pago = new Pago
        {
            IdEmpresa = idEmpresa,
            IdCuentaTicket = idCuentaTicket,
            MetodoPago = request.MetodoPago,
            Monto = cuenta.Total,
            FechaPago = DateTime.UtcNow
        };

        _context.Pagos.Add(pago);

        cuenta.Estado = "PAGADO";
        cuenta.FechaPago = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "Cuenta pagada exitosamente", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }

    public async Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return null;

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .Select(i => new CuentaTicketItemResponse
            {
                IdCuentaTicketItem = i.IdCuentaTicketItem,
                IdProducto = i.IdProducto,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal = i.Subtotal,
                Nota = i.Nota,
                EstadoComanda = i.EstadoComanda
            })
            .ToListAsync();

        return new CuentaTicketResponse
        {
            IdCuentaTicket = cuenta.IdCuentaTicket,
            Numero = cuenta.Numero,
            Estado = cuenta.Estado,
            IdAlmacen = cuenta.IdAlmacen,
            Mesero = cuenta.Mesero,
            Subtotal = cuenta.Subtotal,
            Impuesto = cuenta.Impuesto,
            Total = cuenta.Total,
            FechaCreacion = cuenta.FechaCreacion,
            FechaPago = cuenta.FechaPago,
            Items = items
        };
    }

    public async Task<IEnumerable<CuentaTicketResponse>> ListarAbiertasAsync(int idEmpresa)
    {
        var cuentas = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == idEmpresa && c.Estado == "ABIERTO")
            .ToListAsync();

        var result = new List<CuentaTicketResponse>();
        foreach (var c in cuentas)
        {
            var res = await ObtenerCuentaAsync(c.IdCuentaTicket, idEmpresa);
            if (res != null) result.Add(res);
        }

        return result;
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ProcesarComandaAsync(int idCuentaTicket, int idEmpresa)
    {
        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket && i.EstadoComanda == "NUEVO")
            .ToListAsync();

        foreach (var item in items)
        {
            item.EstadoComanda = "PENDIENTE";
        }

        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateKds");
        
        return (true, "Comanda enviada a cocina", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ActualizarMeseroAsync(int idCuentaTicket, string nuevoMesero, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return (false, "Cuenta no encontrada", null);
        
        cuenta.Mesero = nuevoMesero;
        await _context.SaveChangesAsync();

        return (true, "Mesero actualizado", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarEstadoItemAsync(int idItem, string nuevoEstado, int idEmpresa)
    {
        var item = await _context.CuentasTicketItems.FindAsync(idItem);
        if (item == null) return (false, "Item no encontrado");

        item.EstadoComanda = nuevoEstado;
        await _context.SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateKds");
        
        return (true, "Estado de item actualizado");
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CancelarCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return (false, "Cuenta no encontrada", null);
        if (cuenta.Estado != "ABIERTO") return (false, "Solo se pueden cancelar cuentas abiertas", null);

        cuenta.Estado = "CANCELADO";
        await _context.SaveChangesAsync();

        return (true, "Cuenta cancelada", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ReenviarComandaAsync(int idCuentaTicket, int idEmpresa)
    {
        return (true, "Comanda reenviada", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
    }
}
