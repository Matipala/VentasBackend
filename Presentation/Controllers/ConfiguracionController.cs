using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;
using VentasBackend.Domain.Entities;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/configuracion")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionService _configService;

    public ConfiguracionController(IConfiguracionService configService)
    {
        _configService = configService;
    }

    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        return Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string companyCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var config = await _configService.ObtenerConfiguracionAsync(empresaId);
        return Ok(config);
    }

    [HttpPut]
    public async Task<IActionResult> Update(string companyCen, [FromBody] ConfiguracionVentas request)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var config = await _configService.ActualizarConfiguracionAsync(empresaId, request);
        return Ok(config);
    }
}
