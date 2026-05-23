using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using VentasBackend.Application.DTOs;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/tickets")]
public class CuentasTicketsController : ControllerBase
{
    private readonly ICuentaTicketService _cuentaTicketService;

    public CuentasTicketsController(ICuentaTicketService cuentaTicketService)
    {
        _cuentaTicketService = cuentaTicketService;
    }

    private int ResolveCompanyId(string companyCen)
    {
        if (int.TryParse(companyCen, out int id)) return id;
        return 0; // Or throw
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetAbiertas(string companyCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var cuentas = await _cuentaTicketService.ListarAbiertasAsync(empresaId);
        return Ok(cuentas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(string companyCen, [FromBody] CrearCuentaTicketRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.CrearCuentaAsync(request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    private int ResolveTicketId(string ticketCen)
    {
        if (int.TryParse(ticketCen, out int id)) return id;
        return 0;
    }

    [HttpPost("{ticketCen}/items")]
    public async Task<IActionResult> AgregarItem(string companyCen, string ticketCen, [FromBody] AgregarCuentaTicketItemRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var idCuentaTicket = ResolveTicketId(ticketCen);
        var result = await _cuentaTicketService.AgregarItemAsync(idCuentaTicket, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{ticketCen}/payment")]
    public async Task<IActionResult> Pagar(string companyCen, string ticketCen, [FromBody] PagarCuentaTicketRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var idCuentaTicket = ResolveTicketId(ticketCen);
        var result = await _cuentaTicketService.PagarCuentaAsync(idCuentaTicket, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{ticketCen}/order")]
    public async Task<IActionResult> EnviarComanda(string companyCen, string ticketCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var idCuentaTicket = ResolveTicketId(ticketCen);
        var result = await _cuentaTicketService.ProcesarComandaAsync(idCuentaTicket, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpGet("{ticketCen}")]
    public async Task<IActionResult> GetById(string companyCen, string ticketCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var idCuentaTicket = ResolveTicketId(ticketCen);
        var cuenta = await _cuentaTicketService.ObtenerCuentaAsync(idCuentaTicket, empresaId);

        if (cuenta == null)
            return NotFound(new { mensaje = "Cuenta no encontrada." });

        return Ok(cuenta);
    }
}