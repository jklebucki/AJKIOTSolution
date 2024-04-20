using AJKIOT.Shared.Models;

namespace AJKIOT.Api.Hubs
{
    public interface INotificationHub
    {
        Task SendMessage(string clientId, string message);
        Task DeviceUpdated(string clientId, IotDevice updatedDevice);
    }
}
