using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.SignalR;

namespace AJKIOT.Api.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }

        public async Task UpdateDevice(IotDevice updatedDevice)
        {
            await Clients.All.SendAsync("DeviceUpdated", updatedDevice);
        }
    }
}
