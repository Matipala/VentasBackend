using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VentasBackend.Application.DTOs;
using VentasBackend.Application.Interface;
using VentasBackend.Infrastructure.Data;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/sales")]
public class VentasContractController : ControllerBase
{
    private readonly VentasDbContext _context;
    private readonly ICuentaTicketService _cuentaTicketService;
    private readonly IInventoryClient _inventoryClient;

    public VentasContractController(
        VentasDbContext context,
        ICuentaTicketService cuentaTicketService,
        IInventoryClient inventoryClient)
    {
        _context = context;
        _cuentaTicketService = cuentaTicketService;
        _inventoryClient = inventoryClient;
    }

    private Guid ResolveId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out var id)) return id;
        return Guid.Empty;
    }

    private async Task<string> ResolveProductName(string companyCen, string productCen)
    {
        try
        {
            var names = await _inventoryClient.LookupProductNamesAsync(companyCen, new List<string> { productCen });
            return names.GetValueOrDefault(productCen, $"Product-{productCen}");
        }
        catch
        {
            return $"Product-{productCen}";
        }
    }

    [HttpGet("companies/{companyCen}/catalog/products")]
    public async Task<ActionResult<IEnumerable<SellableProductContractDto>>> GetCatalogProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        try
        {
            var items = await _inventoryClient.GetSellableProductsAsync(companyCen, search, categoryCen, warehouseCen, onlyAvailable);
            return Ok(items);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching sellable products from inventory: {ex.Message}");
            return Ok(Array.Empty<SellableProductContractDto>());
        }
    }

    [HttpGet("payment-methods")]
    public async Task<ActionResult<IEnumerable<PaymentMethodContractResponse>>> GetPaymentMethods()
    {
        var metodos = await _context.Pagos
            .Select(p => p.MetodoPago)
            .Distinct()
            .ToListAsync();

        if (!metodos.Any())
        {
            metodos = new List<string> { "EFECTIVO", "TARJETA", "TRANSFERENCIA" };
        }

        return Ok(metodos.Select((m, i) => new PaymentMethodContractResponse
        {
            PaymentMethodCode = m,
            Name = m,
            IsActive = true
        }));
    }

    [HttpGet("companies/{companyCen}/waiters")]
    public async Task<ActionResult<IEnumerable<WaiterContractResponse>>> GetWaiters(string companyCen)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        var meseros = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == empresaId && !string.IsNullOrEmpty(c.Mesero))
            .Select(c => c.Mesero)
            .Distinct()
            .ToListAsync();

        return Ok(meseros.Select((m, i) => new WaiterContractResponse
        {
            WaiterCen = $"waiter-{i + 1}",
            Name = m
        }));
    }

    [HttpGet("companies/{companyCen}/kds/teams")]
    public async Task<ActionResult<IEnumerable<KdsTeamContractResponse>>> GetKdsTeams(string companyCen)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        try
        {
            var products = await _inventoryClient.GetSellableProductsAsync(companyCen, null, null, null, false);
            var stations = products
                .Where(p => !string.IsNullOrEmpty(p.StationCode))
                .Select(p => p.StationCode!)
                .Distinct()
                .ToList();

            if (!stations.Any())
                stations = new List<string> { "COCINA", "BAR" };

            return Ok(stations.Select((s, i) => new KdsTeamContractResponse
            {
                TeamCen = $"team-{i + 1}",
                Name = s,
                CategoryCens = new List<string>()
            }));
        }
        catch
        {
            return Ok(new[]
            {
                new KdsTeamContractResponse { TeamCen = "team-1", Name = "COCINA", CategoryCens = new List<string>() },
                new KdsTeamContractResponse { TeamCen = "team-2", Name = "BAR", CategoryCens = new List<string>() }
            });
        }
    }

    [HttpGet("companies/{companyCen}/kds/teams/{teamCen}/items")]
    public async Task<ActionResult<IEnumerable<KdsItemContractResponse>>> GetKdsTeamItems(
        string companyCen, string teamCen)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        var estacion = teamCen switch
        {
            "team-1" => "COCINA",
            "team-2" => "BAR",
            _ => null
        };

        var items = await _context.CuentasTicketItems
            .Join(_context.CuentasTickets,
                i => i.IdCuentaTicket,
                t => t.IdCuentaTicket,
                (i, t) => new { Item = i, Ticket = t })
            .Where(x => x.Ticket.IdEmpresa == empresaId
                && x.Ticket.Estado == "ABIERTO"
                && x.Item.EstadoComanda != "NUEVO")
            .OrderByDescending(x => x.Item.IdCuentaTicketItem)
            .Take(50)
            .Select(x => x.Item)
            .ToListAsync();

        var productCens = items.Select(i => i.IdProducto.ToString()).Distinct().ToList();
        var productNames = await _inventoryClient.LookupProductNamesAsync(companyCen, productCens);

        return Ok(items.Select(i => new KdsItemContractResponse
        {
            TicketItemCen = i.IdCuentaTicketItem.ToString(),
            TicketCen = i.IdCuentaTicket.ToString(),
            ProductCen = i.IdProducto.ToString(),
            ProductName = productNames.GetValueOrDefault(i.IdProducto.ToString(), $"Product-{i.IdProducto}"),
            Quantity = i.Cantidad,
            Status = i.EstadoComanda,
            Note = i.Nota,
            ResendCount = 0,
            CreatedAt = DateTime.UtcNow.ToString("o")
        }));
    }

    [HttpGet("companies/{companyCen}/tickets/{ticketCen}/items")]
    public async Task<ActionResult<IEnumerable<TicketItemContractResponse>>> GetTicketItems(
        string companyCen, string ticketCen)
    {
        if (!Guid.TryParse(ticketCen, out var ticketId))
            return BadRequest(new { mensaje = "ticketCen invalido" });

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == ticketId)
            .ToListAsync();

        var productCens = items.Select(i => i.IdProducto.ToString()).Distinct().ToList();
        var productNames = await _inventoryClient.LookupProductNamesAsync(companyCen, productCens);

        return Ok(items.Select(i => new TicketItemContractResponse
        {
            TicketItemCen = i.IdCuentaTicketItem.ToString(),
            ProductCen = i.IdProducto.ToString(),
            ProductName = productNames.GetValueOrDefault(i.IdProducto.ToString(), $"Product-{i.IdProducto}"),
            Quantity = i.Cantidad,
            UnitPrice = i.PrecioUnitario,
            Note = i.Nota,
            Status = i.EstadoComanda,
            ResendCount = 0
        }));
    }

    [HttpPatch("companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}")]
    public async Task<ActionResult<TicketItemContractResponse>> UpdateTicketItem(
        string companyCen, string ticketCen, string ticketItemCen,
        [FromBody] UpdateTicketItemContractRequest request)
    {
        if (!Guid.TryParse(ticketItemCen, out var itemId))
            return BadRequest(new { mensaje = "ticketItemCen invalido" });

        var item = await _context.CuentasTicketItems.FindAsync(itemId);
        if (item == null)
            return NotFound(new { mensaje = "Item no encontrado" });

        item.Cantidad = request.Quantity;
        if (request.Note != null)
            item.Nota = request.Note;

        await _context.SaveChangesAsync();

        return Ok(new TicketItemContractResponse
        {
            TicketItemCen = item.IdCuentaTicketItem.ToString(),
            ProductCen = item.IdProducto.ToString(),
            ProductName = await ResolveProductName(companyCen, item.IdProducto.ToString()),
            Quantity = item.Cantidad,
            UnitPrice = item.PrecioUnitario,
            Note = item.Nota,
            Status = item.EstadoComanda
        });
    }

    [HttpPost("companies/{companyCen}/tickets/{ticketCen}/items/{ticketItemCen}/resend")]
    public async Task<ActionResult<TicketItemContractResponse>> ResendTicketItem(
        string companyCen, string ticketCen, string ticketItemCen)
    {
        if (!Guid.TryParse(ticketItemCen, out var itemId))
            return BadRequest(new { mensaje = "ticketItemCen invalido" });

        var item = await _context.CuentasTicketItems.FindAsync(itemId);
        if (item == null)
            return NotFound(new { mensaje = "Item no encontrado" });

        item.EstadoComanda = "PENDIENTE";
        await _context.SaveChangesAsync();

        return Ok(new TicketItemContractResponse
        {
            TicketItemCen = item.IdCuentaTicketItem.ToString(),
            ProductCen = item.IdProducto.ToString(),
            ProductName = await ResolveProductName(companyCen, item.IdProducto.ToString()),
            Quantity = item.Cantidad,
            UnitPrice = item.PrecioUnitario,
            Note = item.Nota,
            Status = item.EstadoComanda
        });
    }

    [HttpGet("companies/{companyCen}/tickets/{ticketCen}/totals")]
    public async Task<ActionResult<TicketTotalsContractResponse>> GetTicketTotals(
        string companyCen, string ticketCen)
    {
        if (!Guid.TryParse(ticketCen, out var ticketId))
            return BadRequest(new { mensaje = "ticketCen invalido" });

        var cuenta = await _context.CuentasTickets.FindAsync(ticketId);
        if (cuenta == null)
            return NotFound(new { mensaje = "Ticket no encontrado" });

        return Ok(new TicketTotalsContractResponse
        {
            TicketCen = cuenta.IdCuentaTicket.ToString(),
            Subtotal = cuenta.Subtotal,
            TaxAmount = cuenta.Impuesto,
            Total = cuenta.Total
        });
    }

    [HttpGet("companies/{companyCen}/tickets/{ticketCen}/print")]
    public async Task<IActionResult> PrintTicket(string companyCen, string ticketCen)
    {
        if (!Guid.TryParse(ticketCen, out var ticketId))
            return BadRequest(new { mensaje = "ticketCen invalido" });

        var cuenta = await _context.CuentasTickets.FindAsync(ticketId);
        if (cuenta == null)
            return NotFound(new { mensaje = "Ticket no encontrado" });

        var texto = $"Ticket #{cuenta.Numero}\nTotal: {cuenta.Total:C2}\n";
        var bytes = System.Text.Encoding.UTF8.GetBytes(texto);

        return File(bytes, "text/plain", $"ticket-{ticketCen}.txt");
    }

    [HttpGet("companies/{companyCen}/tax-configuration")]
    public async Task<ActionResult<TaxConfigurationContractResponse>> GetTaxConfiguration(string companyCen)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        var config = await _context.Configuracion
            .FirstOrDefaultAsync(c => c.IdEmpresa == empresaId);

        return Ok(new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config?.PorcentajeImpuesto ?? 0
        });
    }

    [HttpPut("companies/{companyCen}/tax-configuration")]
    public async Task<ActionResult<TaxConfigurationContractResponse>> UpdateTaxConfiguration(
        string companyCen, [FromBody] UpdateTaxConfigurationContractRequest request)
    {
        var empresaId = ResolveId(companyCen);
        if (empresaId == Guid.Empty)
            return BadRequest(new { mensaje = "companyCen invalido" });

        var config = await _context.Configuracion
            .FirstOrDefaultAsync(c => c.IdEmpresa == empresaId);

        if (config == null)
        {
            config = new Domain.Entities.ConfiguracionVentas
            {
                IdEmpresa = empresaId,
                PorcentajeImpuesto = request.GlobalTaxPercentage,
                NombreImpuesto = "IVA"
            };
            _context.Configuracion.Add(config);
        }
        else
        {
            config.PorcentajeImpuesto = request.GlobalTaxPercentage;
        }

        await _context.SaveChangesAsync();

        return Ok(new TaxConfigurationContractResponse
        {
            CompanyCen = companyCen,
            GlobalTaxPercentage = config.PorcentajeImpuesto
        });
    }
}
