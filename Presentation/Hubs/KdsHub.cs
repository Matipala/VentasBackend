using Microsoft.AspNetCore.SignalR;

namespace VentasBackend.Presentation.Hubs;

public class KdsHub : Hub
{
    public async Task SubscribeToEmpresa(int idEmpresa)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Empresa_{idEmpresa}");
    }

    public async Task UnsubscribeFromEmpresa(int idEmpresa)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Empresa_{idEmpresa}");
    }
}
