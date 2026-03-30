# InventorySaaS - Ventas

Servicio especializado en la gestión de transacciones comerciales, procesamiento de pagos y generación de documentos de venta para el ecosistema InventorySaaS.

## Descripción breve
**InventorySaaS - Ventas** es un módulo independiente que gestiona el ciclo final de la venta. Se encarga de la interacción con los clientes en el punto de venta, la validación de stock a través de una integración con el Core API, y la emisión de tickets de venta con configuración fiscal parametrizable.

## Alcance funcional implementado
Procesamiento integral de ventas, desde la selección de artículos hasta el registro del pago y la emisión del comprobante, asegurando la consistencia de los datos financieros y de inventario.

## Lista de funcionalidades desarrolladas
- **Procesamiento de Tickets:** Generación automática de comprobantes de venta con desglose de ítems.
- **Gestión de Pagos:** Registro y validación de diferentes métodos de pago.
- **Integración con Inventario:** Validación y descuento de stock en tiempo real mediante `IStockGateway` conectado al Core API.
- **Configuración Fiscal:** Manejo de porcentajes de impuestos (IVA/IT) configurables por empresa.
- **Directorio de Clientes:** Gestión de información de clientes para la emisión de facturas/tickets personalizados.
- **Validación de Reglas de Negocio:** Prevención de ventas sin stock suficiente y validación de montos totales.

## 🛠️ Tecnologías utilizadas
- **Runtime:** [.NET 9.0](https://dotnet.microsoft.com/)
- **ORM:** [Entity Framework Core 9.0](https://learn.microsoft.com/ef/core/)
- **Base de Datos:** [PostgreSQL](https://www.postgresql.org/)
- **Comunicación entre servicios:** HTTP Client / Gateways personalizados.
- **Documentación:** [Swashbuckle (Swagger)](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## Instrucciones de ejecución

### Requisitos previos
- .NET 9.0 SDK
- PostgreSQL 15+
- **Nota:** La [Core API (Inventario)](file:///Users/josue/Documents/7moSemestre/TallerSoftwarell/InventorySaaSBackend/README.md) debe estar activa para validar stock.

### Pasos para iniciar el servicio
1. **Configurar la base de datos e integración:**
   Asegúrate de configurar la conexión y la URL del API de Inventario en `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=tulocal;Database=tudb;Username=tuusuario;Password=tu_pass"
     },
     "InventoryApi": {
       "BaseUrl": "http://localhost:5140"
     },
     "Sales": {
       "GlobalTaxPercent": 13
     }
   }
   ```
2. **Restaurar dependencias:**
   ```bash
   dotnet restore
   ```
3. **Aplicar migraciones:**
   ```bash
   dotnet ef database update
   ```
4. **Ejecutar la aplicación:**
   ```bash
   dotnet run
   ```
   Accede a la documentación interactiva en `/swagger` para probar los endpoints de venta.

## 📂 Estructura general del repositorio
```text
├── Application/    # Interfaces de servicios y lógica de orquestación
├── Domain/         # 
│   ├── Entities/   # Definición de tablas (Venta, Pago, Cliente, Ticket)
│   ├── DTOs/       # Objetos de transferencia para requests/responses
│   └── Data/       # Contexto de base de datos
├── Infrastructure/ # Implementaciones de gateways para inventario y persistencia
├── Presentation/   # Endpoints de la API y controladores
└── Migrations/     # Historial de cambios en la base de datos
```
