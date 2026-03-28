using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using System.Text.Json.Serialization;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/ventas/kds")]
public class KdsController : BaseController
{
    private readonly IKdsService _kdsService;
    private readonly ICuentaTicketService _cuentaTicketService;

    public KdsController(IKdsService kdsService, ICuentaTicketService cuentaTicketService)
    {
        _kdsService = kdsService;
        _cuentaTicketService = cuentaTicketService;
    }

    [HttpGet("{estacion}")]
    public async Task<IActionResult> GetItemsByEstacion(string estacion)
    {
        var empresaId = GetEmpresaId();
        var items = await _kdsService.GetItemsPendientesAsync(empresaId, estacion);
        return Ok(items);
    }

    [HttpPatch("items/{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoItemRequest request)
    {
        var empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.ActualizarEstadoItemAsync(id, request.NuevoEstado, empresaId);
        
        if (!result.Exito)
            return BadRequest(result.Mensaje);
            
        return Ok(new { mensaje = result.Mensaje });
    }
}

public class ActualizarEstadoItemRequest
{
    [JsonPropertyName("nuevoEstado")]
    public string NuevoEstado { get; set; } = string.Empty;
}
