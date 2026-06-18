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

    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        return Guid.Empty;
    }

    [HttpGet]
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string companyCen, Guid id)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var cuenta = await _cuentaTicketService.ObtenerCuentaAsync(id, empresaId);

        if (cuenta == null)
            return NotFound(new { mensaje = "Cuenta no encontrada." });

        return Ok(cuenta);
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AgregarItem(string companyCen, Guid id, [FromBody] AgregarCuentaTicketItemRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.AgregarItemAsync(id, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{id}/payment")]
    public async Task<IActionResult> Pagar(string companyCen, Guid id, [FromBody] PagarCuentaTicketRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.PagarCuentaAsync(id, request, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{id}/send")]
    public async Task<IActionResult> EnviarComanda(string companyCen, Guid id)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.ProcesarComandaAsync(id, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPatch("{id}/waiter")]
    public async Task<IActionResult> ActualizarMesero(string companyCen, Guid id, [FromBody] UpdateMeseroRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.ActualizarMeseroAsync(id, request.NuevoMesero, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancelar(string companyCen, Guid id)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.CancelarCuentaAsync(id, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }

    [HttpPost("{id}/resend")]
    public async Task<IActionResult> ReenviarComanda(string companyCen, Guid id)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var result = await _cuentaTicketService.ReenviarComandaAsync(id, empresaId);

        if (!result.Exito)
            return BadRequest(new { mensaje = result.Mensaje });

        return Ok(result.Cuenta);
    }
}
