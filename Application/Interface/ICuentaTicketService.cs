using VentasBackend.Application.DTOs;

namespace VentasBackend.Application.Interface;

public interface ICuentaTicketService
{
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CrearCuentaAsync(CrearCuentaTicketRequest request, Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> AgregarItemAsync(Guid idCuentaTicket, AgregarCuentaTicketItemRequest request, Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> PagarCuentaAsync(Guid idCuentaTicket, PagarCuentaTicketRequest request, Guid idEmpresa);
    Task<CuentaTicketResponse?> ObtenerCuentaAsync(Guid idCuentaTicket, Guid idEmpresa);
    Task<IEnumerable<CuentaTicketResponse>> ListarAbiertasAsync(Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ProcesarComandaAsync(Guid idCuentaTicket, Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ActualizarMeseroAsync(Guid idCuentaTicket, string nuevoMesero, Guid idEmpresa);
    Task<(bool Exito, string Mensaje)> ActualizarEstadoItemAsync(Guid idItem, string nuevoEstado, Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> CancelarCuentaAsync(Guid idCuentaTicket, Guid idEmpresa);
    Task<(bool Exito, string Mensaje, CuentaTicketResponse? Cuenta)> ReenviarComandaAsync(Guid idCuentaTicket, Guid idEmpresa);
}
