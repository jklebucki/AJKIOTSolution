using AJKIOT.Shared.Models;
using System.Net.WebSockets;

namespace AJKIOT.Api.Services
{
    public interface IDeviceStatusService
    {
        IotDevice? GetDeviceStatus(int? deviceId);
        IEnumerable<IotDevice> GetAllDevices();
        void SetDeviceStatus(IotDevice deviceStatus);
        void ChangePinStatus(int deviceId, int status);
        Task MessageClient(WebSocket webSocket, IDeviceStatusService _statusService);
        IotDevice MessageToDeviceStatus(string message);
    }
}
