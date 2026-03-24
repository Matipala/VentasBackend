using VentasBackend.Domain.DTOs;

namespace VentasBackend.Business.Interface;

public interface ICuentaTicketService
{
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CrearCuentaAsync(CrearCuentaTicketRequest request, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> AgregarItemAsync(int idCuentaTicket, AgregarCuentaTicketItemRequest request, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(int idCuentaTicket, PagarCuentaTicketRequest request, int idEmpresa);
    Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa);
}