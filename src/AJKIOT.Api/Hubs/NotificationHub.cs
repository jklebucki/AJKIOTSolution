using System.Collections.Concurrent;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AJKIOT.Api.Hubs
{
    public class NotificationHub : Hub
    {
        private static ConcurrentDictionary<string, string> _clientConnections = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var clientId = Context.GetHttpContext()!.Request.Query["clientId"];
            Console.WriteLine($"Connected {clientId}");
            if (!string.IsNullOrEmpty(clientId))
            {
                _clientConnections[Context.ConnectionId] = clientId!;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _clientConnections.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string clientId, string message)
        {
            var targetClientId = _clientConnections.FirstOrDefault(c => c.Value == clientId).Value;
            if (targetClientId != null)
            {
                await Clients.Client(targetClientId).SendAsync("ReceiveMessage", message);
            }
        }

        public async Task DeviceUpdated(string clientId, IotDevice updatedDevice)
        {
            Console.WriteLine($"Message to {clientId}: {updatedDevice.DeviceName}");
            var targetClientId = _clientConnections.FirstOrDefault(c => c.Value == clientId).Value;
            if (targetClientId != null)
            {

                await Clients.Client(targetClientId).SendAsync("DeviceUpdated", updatedDevice);
            }

        }
    }
}
