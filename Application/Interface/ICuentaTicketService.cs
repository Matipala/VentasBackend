using VentasBackend.Application.DTOs;

namespace VentasBackend.Application.Interface;

public interface ICuentaTicketService
{
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CrearCuentaAsync(CrearCuentaTicketRequest request, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> AgregarItemAsync(int idCuentaTicket, AgregarCuentaTicketItemRequest request, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(int idCuentaTicket, PagarCuentaTicketRequest request, int idEmpresa);
    Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa);
    Task<IEnumerable<CuentaTicketResponse>> ListarAbiertasAsync(int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ProcesarComandaAsync(int idCuentaTicket, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ActualizarMeseroAsync(int idCuentaTicket, string nuevoMesero, int idEmpresa);
    Task<(bool Exito, string Mensaje)> ActualizarEstadoItemAsync(int idItem, string nuevoEstado, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CancelarCuentaAsync(int idCuentaTicket, int idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ReenviarComandaAsync(int idCuentaTicket, int idEmpresa);
}