using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AJKIOT.Api.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ConnectionMapping _connectedClients;

        public NotificationHub(ConnectionMapping connectedClients)
        {
            _connectedClients = connectedClients;
        }

        public override Task OnConnectedAsync()
        {

            var clientId = Context.GetHttpContext()!.Request.Query["clientId"].ToString();
            Console.WriteLine($"Client connected: {clientId}");
            if (!string.IsNullOrEmpty(clientId))
            {
                _connectedClients.Add(Context.ConnectionId, clientId);
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectedClients.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyDeviceUpdate(string clientId, IotDevice updatedDevice)
        {
            foreach (var connection in _connectedClients.GetAllClients().Where(c => c.Value == clientId))
            {
                await Clients.Client(connection.Key).SendAsync("DeviceUpdated", updatedDevice);
            }
        }
    }
}
