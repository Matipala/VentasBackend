using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using VentasBackend.Application.DTOs;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/ventas/cuentas")]
public class CuentasTicketsController : BaseController
{
    private readonly ICuentaTicketService _cuentaTicketService;

    public CuentasTicketsController(ICuentaTicketService cuentaTicketService)
    {
        _cuentaTicketService = cuentaTicketService;
    }

    [HttpGet("abiertas")]
    public async Task<IActionResult> GetAbiertas()
    {
        var empresaId = GetEmpresaId();
        var cuentas = await _cuentaTicketService.ListarAbiertasAsync(empresaId);
        return Ok(cuentas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCuentaTicketRequest request)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.CrearCuentaAsync(request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/items")]
    public async Task<IActionResult> AgregarItem(int idCuentaTicket, [FromBody] AgregarCuentaTicketItemRequest request)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.AgregarItemAsync(idCuentaTicket, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }
    [HttpPost("{idCuentaTicket:int}/pagar")]
    public async Task<IActionResult> Pagar(int idCuentaTicket, [FromBody] PagarCuentaTicketRequest request)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.PagarCuentaAsync(idCuentaTicket, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/comanda")]
    public async Task<IActionResult> EnviarComanda(int idCuentaTicket)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.ProcesarComandaAsync(idCuentaTicket, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPatch("{idCuentaTicket:int}/mesero")]
    public async Task<IActionResult> ActualizarMesero(int idCuentaTicket, [FromBody] UpdateMeseroRequest request)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.ActualizarMeseroAsync(idCuentaTicket, request.NuevoMesero, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int idCuentaTicket)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.CancelarCuentaAsync(idCuentaTicket, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/reenviar-comanda")]
    public async Task<IActionResult> ReenviarComanda(int idCuentaTicket)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.ReenviarComandaAsync(idCuentaTicket, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpGet("{idCuentaTicket:int}")]
    public async Task<IActionResult> GetById(int idCuentaTicket)
    {
        var empresaId = GetEmpresaId();
        var cuenta = await _cuentaTicketService.ObtenerCuentaAsync(idCuentaTicket, empresaId);

        if (cuenta == null)
            return NotFound(new { mensaje = "Cuenta no encontrada." });

        return Ok(cuenta);
    }
}