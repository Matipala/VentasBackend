using Microsoft.AspNetCore.Mvc;

namespace VentasBackend.Presentation.Controllers;

public abstract class BaseController : ControllerBase
{
    protected Guid GetEmpresaId()
    {
        if (!Request.Headers.TryGetValue("x-empresa-id", out var headerValue))
            throw new BadHttpRequestException("Falta el header x-empresa-id.");

        if (!Guid.TryParse(headerValue.FirstOrDefault(), out var empresaId) || empresaId == Guid.Empty)
            throw new BadHttpRequestException("Header x-empresa-id invalido.");

        return empresaId;
    }
}
