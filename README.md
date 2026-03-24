# VentasBackend

Backend del módulo de ventas para InventorySaaS.

## Descripción

Este proyecto implementa la lógica y API REST para la gestión de ventas y su integración con el módulo de inventario. Utiliza ASP.NET Core, Entity Framework Core y PostgreSQL, siguiendo una arquitectura limpia.

## Estructura del proyecto

- **Business/Interface**: Interfaces de servicios de negocio (contratos).
- **Domain/Data**: Contexto de base de datos (DbContext).
- **Domain/DTOs**: Objetos de transferencia de datos (requests/responses).
- **Domain/Entities**: Entidades del dominio (tablas principales).
- **Infrastructure/Configuration**: Clases para configuración tipada.
- **Infrastructure/Services**: Implementaciones de servicios y gateways.
- **Presentation/Controllers**: Controladores de la API (endpoints HTTP).

## Configuración

1. Edita `appsettings.json` con tus datos de conexión y la URL del backend de inventario:

	 ```json
	 {
		 "ConnectionStrings": {
			 "DefaultConnection": "Host=tulocalhost;Port=tupuerto;Database=tudatabase;Username=tuuser;Password=tupassword"
		 },
		 "InventoryApi": {
			 "BaseUrl": "http://localhost:0000"
		 },
		 "Sales": {
			 "GlobalTaxPercent": 13
		 }
	 }
	 ```

2. Aplica las migraciones de la base de datos:

	 ```
	 dotnet ef database update
	 ```

## Ejecución

1. Restaura y compila el proyecto:

	 ```
	 dotnet restore
	 dotnet build
	 ```

2. Ejecuta el backend:

	 ```
	 dotnet run
	 ```

3. La API estará disponible en `https://localhost:0000` (o el puerto configurado).


## Integración con Inventario

El servicio utiliza un gateway HTTP (`IStockGateway`) para validar y descontar stock llamando al backend de inventario. La URL se configura en `appsettings.json`.
