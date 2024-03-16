using AJKIOT.Shared.Models;
using System.Net.WebSockets;

namespace AJKIOT.Api.Services
{
    public interface IDeviceStatusService
    {
        DeviceStatus? GetDeviceStatus(int? deviceId);
        IEnumerable<DeviceStatus> GetAllDevices();
        void SetDeviceStatus(DeviceStatus deviceStatus);
        void ChangePinStatus(int deviceId, int status);
        Task MessageClient(WebSocket webSocket, IDeviceStatusService _statusService);
        DeviceStatus MessageToDeviceStatus(string message);
    }
}
