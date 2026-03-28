using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using VentasBackend.Domain.Entities;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/ventas/configuracion")]
public class ConfiguracionController : BaseController
{
    private readonly IConfiguracionService _configService;

    public ConfiguracionController(IConfiguracionService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var empresaId = GetEmpresaId();
        var config = await _configService.ObtenerConfiguracionAsync(empresaId);
        return Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ConfiguracionVentas request)
    {
        var empresaId = GetEmpresaId();
        var config = await _configService.ActualizarConfiguracionAsync(empresaId, request);
        return Ok(config);
    }
}
