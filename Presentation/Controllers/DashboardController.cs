using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/ventas/dashboard")]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("resumen-diario")]
    public async Task<IActionResult> GetResumenDiario()
    {
        var empresaId = GetEmpresaId();
        var resumen = await _dashboardService.GetResumenDiarioAsync(empresaId);
        return Ok(resumen);
    }

    [HttpGet("top-productos")]
    public async Task<IActionResult> GetTopProductos()
    {
        var empresaId = GetEmpresaId();
        var top = await _dashboardService.GetTopProductosAsync(empresaId);
        return Ok(top);
    }

    [HttpGet("carga-kds")]
    public async Task<IActionResult> GetCargaKds()
    {
        var empresaId = GetEmpresaId();
        var stats = await _dashboardService.GetCargaKdsAsync(empresaId);
        return Ok(stats);
    }
}
