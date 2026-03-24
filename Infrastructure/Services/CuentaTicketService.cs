using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VentasBackend.Business.Interface;
using VentasBackend.Data;
using VentasBackend.Domain.DTOs;
using VentasBackend.Domain.Entities;
using VentasBackend.Infrastructure.Configuration;

namespace VentasBackend.Infrastructure.Services;

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

        return (true, "Cuenta creada exitosamente.", MapCuenta(cuenta));
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
        bool stockValido = await _stockGateway.ValidarStockAsync(request.IdProducto, cuenta.IdAlmacen, request.Cantidad);
        if (!stockValido)
            return (false, "No hay suficiente stock para el producto solicitado.", null);

        var item = new CuentaTicketItem
        {
            IdCuentaTicket = cuenta.IdCuentaTicket,
            IdProducto = request.IdProducto,
            Cantidad = request.Cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = precioUnitario * request.Cantidad,
            Nota = request.Nota,
            ComandaEnviada = false
        };

        _context.CuentasTicketItems.Add(item);
        await _context.SaveChangesAsync();

        await RecalcularTotalesAsync(cuenta);

        return (true, "Item agregado exitosamente.", MapCuenta(cuenta));
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

        var tieneItems = await _context.CuentasTicketItems.AnyAsync(i => i.IdCuentaTicket == idCuentaTicket);
        if (!tieneItems)
            return (false, "No se puede pagar una cuenta sin items.", null);

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

        return (true, "Pago registrado exitosamente.", MapCuenta(cuenta));
    }

    public async Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        return cuenta == null ? null : MapCuenta(cuenta);
    }

    private async Task RecalcularTotalesAsync(CuentaTicket cuenta)
    {
        var subtotal = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == cuenta.IdCuentaTicket)
            .SumAsync(i => i.Subtotal);

        var impuesto = Math.Round(subtotal * (_salesOptions.GlobalTaxPercent / 100m), 2, MidpointRounding.AwayFromZero);

        cuenta.Subtotal = subtotal;
        cuenta.Impuesto = impuesto;
        cuenta.Total = subtotal + impuesto;

        await _context.SaveChangesAsync();
    }

    private static CuentaTicketResponse MapCuenta(CuentaTicket cuenta)
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
            FechaPago = cuenta.FechaPago
        };
    }
}