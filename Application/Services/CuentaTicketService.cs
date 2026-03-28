using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Data;
using VentasBackend.Application.DTOs;
using VentasBackend.Domain.Entities;
using VentasBackend.Infrastructure.Configuration;

namespace VentasBackend.Application.Services;

public class CuentaTicketService : ICuentaTicketService
{
    private readonly VentasDbContext _context;
    private readonly SalesOptions _salesOptions;
    private readonly IStockGateway _stockGateway;

    public CuentaTicketService(VentasDbContext context, IOptions<SalesOptions> salesOptions, IStockGateway stockGateway)
    {
        _context = context;
        _salesOptions = salesOptions.Value;
        _stockGateway = stockGateway;
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CrearCuentaAsync(
        CrearCuentaTicketRequest request,
        int idEmpresa)
    {
        if (request.IdAlmacen <= 0)
            return (false, "El almacen es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(request.Mesero))
            return (false, "El mesero es obligatorio.", null);

        var ultimoNumero = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == idEmpresa)
            .Select(c => (int?)c.Numero)
            .MaxAsync() ?? 0;

        var cuenta = new CuentaTicket
        {
            IdEmpresa = idEmpresa,
            IdAlmacen = request.IdAlmacen,
            IdCliente = request.IdCliente,
            Numero = ultimoNumero + 1,
            Mesero = request.Mesero.Trim(),
            Estado = "ABIERTO",
            Subtotal = 0m,
            Impuesto = 0m,
            Total = 0m,
            FechaCreacion = DateTime.UtcNow
        };

        _context.CuentasTickets.Add(cuenta);
        await _context.SaveChangesAsync();

        return (true, "Cuenta creada exitosamente.", MapCuenta(cuenta, new List<CuentaTicketItem>()));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> AgregarItemAsync(
        int idCuentaTicket,
        AgregarCuentaTicketItemRequest request,
        int idEmpresa)
    {
        if (request.Cantidad <= 0)
            return (false, "La cantidad debe ser mayor a cero.", null);

        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo se pueden agregar items en cuentas abiertas.", null);

        var precioUnitario = request.PrecioUnitario ?? 0m;
        if (precioUnitario < 0)
            return (false, "El precio unitario no puede ser negativo.", null);

        // Validar stock antes de agregar el item
        bool stockValido = await _stockGateway.ValidarStockAsync(request.IdProducto, cuenta.IdAlmacen, request.Cantidad, idEmpresa);
        if (!stockValido)
            return (false, "No hay suficiente stock para el producto solicitado.", null);

        var item = new CuentaTicketItem
        {
            IdCuentaTicket = cuenta.IdCuentaTicket,
            IdProducto = request.IdProducto,
            Cantidad = request.Cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = Math.Round(request.Cantidad * precioUnitario, 2, MidpointRounding.AwayFromZero),
            Nota = request.Nota,
            EstadoComanda = "NUEVO"
        };

        _context.CuentasTicketItems.Add(item);
        await _context.SaveChangesAsync();

        await RecalcularTotalesAsync(cuenta);

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return (true, "Item agregado exitosamente.", MapCuenta(cuenta, items));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(
        int idCuentaTicket,
        PagarCuentaTicketRequest request,
        int idEmpresa)
    {
        var metodo = (request.MetodoPago ?? string.Empty).Trim().ToUpperInvariant();
        var metodosValidos = new[] { "EFECTIVO", "QR", "TARJETA" };

        if (!metodosValidos.Contains(metodo))
            return (false, "Metodo de pago invalido. Use: EFECTIVO, QR o TARJETA.", null);

        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo las cuentas abiertas pueden pagarse.", null);

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        if (!items.Any())
            return (false, "No se puede pagar una cuenta sin items.", null);

        // HU-20: Validar stock de todos los productos antes de proceder
        foreach (var item in items)
        {
            bool stockValido = await _stockGateway.ValidarStockAsync(item.IdProducto, cuenta.IdAlmacen, item.Cantidad, idEmpresa);
            if (!stockValido)
                return (false, $"No hay suficiente stock para el producto ID {item.IdProducto}.", null);
        }

        // HU-21: Descontar stock al confirmar el pago
        foreach (var item in items)
        {
            await _stockGateway.DescontarStockAsync(item.IdProducto, cuenta.IdAlmacen, (decimal)item.Cantidad, idEmpresa);
        }

        cuenta.Estado = "PAGADO";
        cuenta.FechaPago = DateTime.UtcNow;

        _context.Pagos.Add(new Pago
        {
            IdEmpresa = idEmpresa,
            IdCuentaTicket = cuenta.IdCuentaTicket,
            MetodoPago = metodo,
            Monto = cuenta.Total,
            FechaPago = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return (true, "Pago registrado exitosamente.", MapCuenta(cuenta, items));
    }

    public async Task<IEnumerable<CuentaTicketResponse>> ListarAbiertasAsync(int idEmpresa)
    {
        var cuentas = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == idEmpresa && c.Estado == "ABIERTO")
            .OrderByDescending(c => c.FechaCreacion)
            .ToListAsync();

        var response = new List<CuentaTicketResponse>();
        foreach (var c in cuentas)
        {
            var items = await _context.CuentasTicketItems
                .Where(i => i.IdCuentaTicket == c.IdCuentaTicket)
                .ToListAsync();
            response.Add(MapCuenta(c, items));
        }

        return response;
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ProcesarComandaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo se pueden enviar comandas de cuentas abiertas.", null);

        var itemsPendientes = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket && i.EstadoComanda == "NUEVO")
            .ToListAsync();

        if (!itemsPendientes.Any())
            return (false, "No hay nuevos ítems para enviar a comanda.", null);

        foreach (var item in itemsPendientes)
        {
            item.EstadoComanda = "PENDIENTE";
        }

        await _context.SaveChangesAsync();
        
        var todosLosItems = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return (true, "Comanda enviada a estaciones.", MapCuenta(cuenta, todosLosItems));
    }

    public async Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null) return null;

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return MapCuenta(cuenta, items);
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ActualizarMeseroAsync(int idCuentaTicket, string nuevoMesero, int idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(nuevoMesero))
            return (false, "El nombre del mesero no puede estar vacío.", null);

        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo se puede cambiar el mesero en cuentas abiertas.", null);

        cuenta.Mesero = nuevoMesero.Trim();
        await _context.SaveChangesAsync();

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return (true, "Mesero actualizado correctamente.", MapCuenta(cuenta, items));
    }

    public async Task<(bool Exito, string Mensaje)> ActualizarEstadoItemAsync(int idItem, string nuevoEstado, int idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(nuevoEstado))
            return (false, "El estado de comanda es requerido.");

        var estadosValidos = new[] { "PENDIENTE", "PREPARACION", "LISTO" };
        if (!estadosValidos.Contains(nuevoEstado.ToUpperInvariant()))
            return (false, "Estado de comanda inválido.");

        var itemContainer = await _context.CuentasTicketItems
            .Join(_context.CuentasTickets, 
                  i => i.IdCuentaTicket, 
                  c => c.IdCuentaTicket, 
                  (i, c) => new { Item = i, Cuenta = c })
            .FirstOrDefaultAsync(x => x.Item.IdCuentaTicketItem == idItem && x.Cuenta.IdEmpresa == idEmpresa);

        if (itemContainer == null)
            return (false, "Ítem no encontrado o no pertenece a la empresa.");

        itemContainer.Item.EstadoComanda = nuevoEstado.ToUpperInvariant();
        await _context.SaveChangesAsync();

        return (true, "Estado de ítem actualizado.");
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CancelarCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo las cuentas abiertas pueden cancelarse.", null);

        cuenta.Estado = "CANCELADO";
        await _context.SaveChangesAsync();

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return (true, "Cuenta cancelada exitosamente.", MapCuenta(cuenta, items));
    }

    public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ReenviarComandaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        if (!items.Any())
            return (false, "No hay productos en esta cuenta para enviar.", null);

        foreach (var item in items)
        {
            if (item.EstadoComanda == "NUEVO")
            {
                item.EstadoComanda = "PENDIENTE";
                // No prefix for new items, they are being sent for the first time
            }
            else 
            {
                // Previously sent items get the prefix and are reset to PENDING
                item.EstadoComanda = "PENDIENTE";
                if (item.Nota == null || !item.Nota.StartsWith("[REENVÍO]"))
                {
                    item.Nota = "[REENVÍO] " + (item.Nota ?? "");
                }
            }
        }

        await _context.SaveChangesAsync();

        var todosLosItems = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        return (true, "Comanda reenviada con éxito.", MapCuenta(cuenta, todosLosItems));
    }

    private async Task RecalcularTotalesAsync(CuentaTicket cuenta)
    {
        var subtotal = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == cuenta.IdCuentaTicket)
            .SumAsync(i => i.Subtotal);

        var config = await _context.Configuracion
            .FirstOrDefaultAsync(c => c.IdEmpresa == cuenta.IdEmpresa);
        
        decimal porcentaje = config?.PorcentajeImpuesto ?? 0m;
        var impuesto = Math.Round(subtotal * (porcentaje / 100m), 2, MidpointRounding.AwayFromZero);

        cuenta.Subtotal = subtotal;
        cuenta.Impuesto = impuesto;
        cuenta.Total = subtotal + impuesto;

        await _context.SaveChangesAsync();
    }

    private static CuentaTicketResponse MapCuenta(CuentaTicket cuenta, List<CuentaTicketItem> items)
    {
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
            Items = items.Select(i => new CuentaTicketItemResponse
            {
                IdCuentaTicketItem = i.IdCuentaTicketItem,
                IdProducto = i.IdProducto,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Subtotal = i.Subtotal,
                Nota = i.Nota,
                EstadoComanda = (i.EstadoComanda ?? "NUEVO").ToUpperInvariant()
            }).ToList()
        };
    }
}