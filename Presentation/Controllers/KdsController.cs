using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using System.Text.Json.Serialization;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/kds")]
public class KdsController : ControllerBase
{
    private readonly IKdsService _kdsService;
    private readonly ICuentaTicketService _cuentaTicketService;

    public KdsController(IKdsService kdsService, ICuentaTicketService cuentaTicketService)
    {
        _kdsService = kdsService;
        _cuentaTicketService = cuentaTicketService;
    }

    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        return Guid.Empty;
    }

    [HttpGet("{estacion}")]
    public async Task<IActionResult> GetItemsByEstacion(string companyCen, string estacion)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var items = await _kdsService.GetItemsPendientesAsync(empresaId, estacion);
        return Ok(items);
    }

    [HttpPatch("items/{id}/status")]
    public async Task<IActionResult> ActualizarEstado(string companyCen, Guid id, [FromBody] ActualizarEstadoItemRequest request)
    {
        var empresaId = ResolveCompanyId(companyCen);
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
