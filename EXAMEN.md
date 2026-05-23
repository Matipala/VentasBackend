# Examen Parcial — Josue Matias Molina Palacios

## Sección 1 — Identificación

- **Nombre completo:** Josue Matias Molina Palacios
- **Pareja asignada para el sábado:** Evert Moreno
- **Repositorio de Inventario:** [https://github.com/Matipala/InventorySaaSBackend.git] 
- **Repositorio de Ventas:** [https://github.com/Matipala/VentasBackend.git] (este mismo)
- **Contrato API acordado en grupo:** [Contrato](./contrato-api.yaml)
- **URL del Swagger autogenerado** (cuando levantás el backend localmente): http://localhost:5005/swagger/index.html

## Sección 2 — Decisiones técnicas con snippets

### 2.1 Árbol de carpetas del backend de Ventas

Pegá la estructura de carpetas de tu proyecto de Ventas. Ejemplo:

```
VentasBackend/
├── Application/
│   ├── DTOs/
│   │   ├── Contract/
│   └── Interface/
├── Domain/
│   └── Entities/
├── Infrastructure/
│   ├── Configuration/
│   ├── Data/
│   ├── Middlewares/
│   └── Services/
├── Migrations/
├── Presentation/
│   ├── Controllers/
│   └── Hubs/
├── Properties/
├── Program.cs
├── VentasBackend.csproj
├── appsettings.json
└── contrato-api.yaml
```

Se organizo siguiendo los principios de **Clean Architecture** para lograr una separacion clara de responsabilidades, el negocio y sus entidades estan en Domain, la lógica y contratos de aplicacion en Application, la persistencia y clientes externos en Infrastructure, y los puntos de entrada HTTP/WebSocke para el manejo en tiempo real de KDS en la Presentation.

### 2.2 Flujo de "registrar una venta"

Pegá los snippets del código que se ejecuta cuando un usuario confirma una venta, en orden:

1. El endpoint que recibe el request (Controller).
```csharp
// Presentation/Controllers/CuentasTicketsController.cs
[HttpPost("{ticketCen}/payment")]
public async Task<IActionResult> Pagar(string companyCen, string ticketCen, [FromBody] PagarCuentaTicketRequest request)
{
    var empresaId = ResolveCompanyId(companyCen);
    var idCuentaTicket = ResolveTicketId(ticketCen);
    var result = await _cuentaTicketService.PagarCuentaAsync(idCuentaTicket, request, empresaId);

    if (!result.Exito)
        return BadRequest(new { mensaje = result.Mensaje });

    return Ok(result.Cuenta);
}
```

2. La capa intermedia que procesa la lógica (Service / Use Case / Handler).
```csharp
// Infrastructure/Services/CuentaTicketService.cs (Parte de la logica)
public async Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(int idCuentaTicket, PagarCuentaTicketRequest request, int idEmpresa)
{
    var cuenta = await _context.CuentasTickets
        .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

    if (cuenta == null) return (false, "Cuenta no encontrada", null);
    if (cuenta.Estado != "ABIERTO") return (false, "La cuenta ya está pagada o cancelada", null);

    var items = await _context.CuentasTicketItems
        .Where(i => i.IdCuentaTicket == idCuentaTicket)
        .ToListAsync();

    if (!items.Any()) return (false, "La cuenta no tiene items para pagar", null);

    // --- INTEGRACION CON INVENTARIO ---
    var companyCen = idEmpresa.ToString();
    var warehouseCen = cuenta.IdAlmacen.ToString();
    var stockItems = items.Select(i => new StockItemDto
    {
        ProductCen = i.IdProducto.ToString(),
        Quantity = i.Cantidad
    }).ToList();

    // 1. Validar Stock con el Inventario
    try
    {
        var validation = await _inventoryClient.ValidateStockAsync(companyCen, warehouseCen, stockItems);
        if (!validation.IsValid)
        {
            var faltantes = string.Join(", ", validation.Requirements.Select(r => $"{r.ProductCen} (Faltan {r.MissingQuantity})"));
            return (false, $"Stock insuficiente en Inventario: {faltantes}", null);
        }
    }
    catch
    {
        return (false, "Error al validar stock con el sistema de inventario.", null);
    }

    // 2. Procesar Pago local (Persistencia)
    var pago = new Pago
    {
        IdEmpresa = idEmpresa,
        IdCuentaTicket = idCuentaTicket,
        MetodoPago = request.MetodoPago,
        Monto = cuenta.Total,
        FechaPago = DateTime.UtcNow
    };

    _context.Pagos.Add(pago);
    cuenta.Estado = "PAGADO";
    cuenta.FechaPago = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // 3. Consumir Stock en Inventario
    try
    {
        await _inventoryClient.ConsumeStockAsync(companyCen, warehouseCen, cuenta.IdCuentaTicket.ToString(), "Venta POS", stockItems);
    }
    catch
    {
        return (false, "Error al consumir stock en el sistema de inventario.", null);
    }

    return (true, "Cuenta pagada exitosamente", await ObtenerCuentaAsync(idCuentaTicket, idEmpresa));
}
```

3. La parte que llama al Inventario del compañero (HttpClient o equivalente).
```csharp
// Infrastructure/Services/InventoryClient.cs
public async Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items)
{
    var request = new
    {
        warehouseCen = warehouseCen,
        source = "SALES_PAYMENT",
        items = items
    };

    var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/validate";
    var response = await _httpClient.PostAsJsonAsync(url, request);

    if (!response.IsSuccessStatusCode)
        return new StockValidationResponseDto { IsValid = false };

    return await response.Content.ReadFromJsonAsync<StockValidationResponseDto>() ?? new StockValidationResponseDto { IsValid = false };
}

public async Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items)
{
    var request = new
    {
        warehouseCen = warehouseCen,
        source = "SALES_PAYMENT",
        referenceCen = referenceCen,
        reason = reason,
        items = items
    };

    var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/consume";
    var response = await _httpClient.PostAsJsonAsync(url, request);

    return response.IsSuccessStatusCode;
}
```

4. La parte que persiste la venta en tu BD.
```csharp
// Se ejecuta dentro de CuentaTicketService.PagarCuentaAsync posterior a validar stock:
var pago = new Pago
{
    IdEmpresa = idEmpresa,
    IdCuentaTicket = idCuentaTicket,
    MetodoPago = request.MetodoPago,
    Monto = cuenta.Total,
    FechaPago = DateTime.UtcNow
};

_context.Pagos.Add(pago);
cuenta.Estado = "PAGADO";
cuenta.FechaPago = DateTime.UtcNow;
await _context.SaveChangesAsync();
```

Dividi las responsabilidades siguiendo los principios de Clean Architecture y Responsabilidad Única (SRP), mi controlador solo recibe peticiones HTTP y maneja códigos de estado, el servicio (`CuentaTicketService`) coordina el caso de uso y reglas de negocio, el cliente HTTP (`InventoryClient`) encapsula los detalles de red e integración externa y EFCore(`VentasDbContext`) maneja la persistencia de forma desacoplada de la logica externa.

### 2.3 Llamada al Inventario del compañero

Pegá el código exacto donde tu Ventas llama al API del Inventario del compañero.

**`CuentaTicketService.cs`**
```csharp
// 1. Validación de stock previa al pago
var validation = await _inventoryClient.ValidateStockAsync(companyCen, warehouseCen, stockItems);

// 2. Consumo de stock posterior al registro del pago local
await _inventoryClient.ConsumeStockAsync(companyCen, warehouseCen, cuenta.IdCuentaTicket.ToString(), "Venta POS", stockItems);
```

**`InventoryClient.cs`**
```csharp
public async Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items)
{
    var request = new
    {
        warehouseCen = warehouseCen,
        source = "SALES_PAYMENT",
        items = items
    };

    var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/validate";
    var response = await _httpClient.PostAsJsonAsync(url, request);

    if (!response.IsSuccessStatusCode)
        return new StockValidationResponseDto { IsValid = false };

    return await response.Content.ReadFromJsonAsync<StockValidationResponseDto>() ?? new StockValidationResponseDto { IsValid = false };
}

public async Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items)
{
    var request = new
    {
        warehouseCen = warehouseCen,
        source = "SALES_PAYMENT",
        referenceCen = referenceCen,
        reason = reason,
        items = items
    };

    var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/consume";
    var response = await _httpClient.PostAsJsonAsync(url, request);

    return response.IsSuccessStatusCode;
}
```

Respondé brevemente:
- ¿Qué pasa si el compañero responde con código 200 OK?
  El flujo se ejecuta con normalidad durante la validacion, si el cuerpo de la respuesta indica isValid = true, se prosigue a persistir el pago y luego se consume el stock exitosamente.
- ¿Qué pasa si responde con 404 o 500?
  * **En validacion** response.IsSuccessStatusCode es false, por lo que el método retorna IsValid = false y la venta es rechazada en el service indicando stock insuficiente.
  * **En consumo** response.IsSuccessStatusCode es false y retorna false, pero al no verificar explicitamente el booleano devuelto en el service la venta continua y finaliza exitosamente.
- ¿Qué pasa si el compañero está caído (timeout)?
  La llamada HTTP lanza una excepcion (HttpRequestException o TaskCanceledException) esta excepcion es capturada por los bloques try-catch del servicio CuentaTicketService.cs en cualquiera de los dos pasos, retornando un error controlado al cliente (Error al validar stock... o Error al consumir stock...) y cancelando la operacion de venta para mantener la consistencia.

### 2.4 Configuración de la URL del compañero

Pegá:
- La línea relevante de tu `.env.example` o `appsettings.json`.
**En `.env.example`:**
```env
InventoryApi__BaseUrl=http://localhost:xxxx
```
- El código que lee esa configuración y la usa para construir la llamada HTTP.
**`InventoryClient.cs`**
```csharp
// Constructor que inyecta IConfiguration para leer la URL base
public InventoryClient(HttpClient httpClient, IConfiguration configuration)
{
    _httpClient = httpClient;
    _baseUrl = configuration["InventoryApi:BaseUrl"];
}

// Fragmento donde se construye la llamada HTTP usando la URL base leída:
var url = $"{_baseUrl}/api/inventory/companies/{companyCen}/stock/validate";
var response = await _httpClient.PostAsJsonAsync(url, request);
```

Cambiaria en la variable InventoryApi__BaseUrl en mi archivo .env con la nueva IP y puerto de mi compañero, sin necesidad de recompilar o modificar el código fuente.

## Sección 3 — Sobre el trabajo en grupo del contrato API

- **3.1** ¿Hubo desacuerdos al definir el contrato? ¿Cuáles?
no hubo, el que tenia mas avance hizo el contrato
- **3.2** ¿Cómo se resolvieron?
se baso en el ejemplo dado en clase
- **3.3** ¿Qué propusiste vos específicamente que quedó en el contrato final?
nada

## Sección 4 — Teoría aplicada

Respondé cada pregunta en 1-2 párrafos. Está permitido usar IA para mejorar redacción, pero las respuestas deben hacer referencia explícita a tu propio código o decisiones.

**4.1** Tu compañero te avisa que va a cambiar el campo `cantidad` por `qty` en su respuesta del endpoint de stock. Tu sistema ya consume ese endpoint. Explicá qué riesgos genera ese cambio y qué prácticas conocés para evitar que un cambio así rompa los sistemas que dependen de su API.
    
El riesgo principal es que mi código se rompa al darme la respuesta. En mi clase InventoryClient.cs, la llamada espera que el JSON contenga exactamente cantidad y no qty, entonces fallará el campo con un valor por defecto, lo que provocaria que mi logica en CuentaTicketService rechace los pagos al asumir que no hay stock suficiente, o tire una excepción HTTP 500.

Para evitar esto, lo mejor es que el compañero implemente versionamiento en su API (por ejemplo, tener una ruta `/v2/` para el cambio mientras mantiene activa la `/v1/` para darme tiempo de actualizar mi código)

**4.2** Tu sistema de Ventas hace una petición al Inventario para descontar stock. La red se cae justo después de que Inventario procesó el descuento pero antes de que la respuesta llegue a Ventas. ¿Qué problema se genera? ¿Cómo lo manejarías?

El problema que se genera entre ambos sistemas. En el inventario de mi compañero el stock ya se descontó, pero de mi lado la venta nunca se completo localmente (se canceló el flujo debido a la excepción de red capturada por el try-catch en CuentaTicketService.cs) si reintentan el pago, se enviará otra petición de descuento al inventario, provocando que se descuente el stock dos veces por una sola venta fisica.

Para manejar esta situacion, lo correcto es implementar **idempotencia** en el endpoint de consumo del inventario. Esto significa que cada solicitud envíe un identificador único (como el ID del ticket de venta, idCuentaTicket). De esa forma, si la red falla y vuelvo a enviar la misma peticion, el inventario del compañero sabra que ya la proceso y devolvera una respuesta exitosa sin volver a descontar el stock.(tuve este problema con una app de banco al procesar un pago se quedo en el modal para confirmar y le aprete 2 veces porque no desaparecia y me cobro 2 veces cuando cerre y volvi abrir la app, por suerte me devolvieron la plata)

**4.3** Si el Inventario del compañero está caído, ¿debería tu Ventas permitir seguir registrando ventas? Justificá considerando ventajas y desventajas de cada postura. ¿Qué hace TU sistema hoy en ese caso?

- Si permito registrar ventas aun con el inventario caído, la ventaja es que el negocio no se detiene y los clientes pueden seguir comprando. La desventaja es que corremos el riesgo de vender productos de los que ya no hay stock físico

- la gran desventaja es que si el sistema del compañero se cae, mi sistema de ventas queda inútil y paraliza la operación del restaurante.

Hoy en día mi sistema bloquea el registro. En `CuentaTicketService.cs` tengo bloques `try-catch` que manejan los fallos en la llamada al inventario. Si la validación o el consumo fallan por un timeout o porque el servicio está caído, la función retorna `false` y no permite guardar el pago localmente, priorizando la consistencia sobre la disponibilidad.

**4.4** Explicá por qué tener la URL del compañero hardcodeada como `http://localhost:5000` es un problema. ¿Cuál es la solución correcta y cómo la implementaste vos?

Tener la URL hardcodeada como http://localhost:5000 es un problema crítico de acoplamiento. Si mi compañero cambia su puerto, levanta su backend en otra máquina en red local, tendríamos que entrar a modificar el código fuente, volver a compilar el proyecto y redesplegarlo, lo cual es ineficiente y peligroso.

La solución correcta es usar variables de entorno. En mi proyecto lo implementé definiendo la variable `InventoryApi__BaseUrl` en el archivo `.env` local, en `Program.cs` se carga este archivo mediante `DotNetEnv.Env.Load()` y luego en `InventoryClient.cs` inyecto `IConfiguration` para leer dinámicamente esa URL asi para cambiar la IP solo edito el archivo `.env` sin modificar nada de código.