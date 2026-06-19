using Microsoft.AspNetCore.Mvc;
using VentasBackend.Application.Interface;

namespace VentasBackend.Presentation.Controllers;

[ApiController]
[Route("api/sales/companies/{companyCen}/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        return Guid.Empty;
    }

    [HttpGet("daily-sales")]
    public async Task<IActionResult> GetDailySales(string companyCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var stats = await _dashboardService.GetResumenDiarioAsync(empresaId);
        return Ok(stats);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts(string companyCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var stats = await _dashboardService.GetTopProductosAsync(empresaId);
        return Ok(stats);
    }

    [HttpGet("kds-status")]
    public async Task<IActionResult> GetKdsStatus(string companyCen)
    {
        var empresaId = ResolveCompanyId(companyCen);
        var stats = await _dashboardService.GetCargaKdsAsync(empresaId);
        return Ok(stats);
    }
}
