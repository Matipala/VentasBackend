using Microsoft.AspNetCore.Mvc;

namespace VentasBackend.Presentation.Controllers;

public abstract class BaseController : ControllerBase
{
    protected int GetEmpresaId()
    {
        if (!Request.Headers.TryGetValue("x-empresa-id", out var headerValue))
            throw new BadHttpRequestException("Falta el header x-empresa-id.");

        if (!int.TryParse(headerValue.FirstOrDefault(), out var empresaId) || empresaId <= 0)
            throw new BadHttpRequestException("Header x-empresa-id invalido.");

        return empresaId;
    }
}