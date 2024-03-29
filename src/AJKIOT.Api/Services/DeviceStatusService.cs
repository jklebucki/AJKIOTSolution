using AJKIOT.Shared.Models;
using System.Net.WebSockets;
using System.Text;

namespace AJKIOT.Api.Services
{
    public class DeviceStatusService : IDeviceStatusService
    {
        private IList<IotDevice> devices = new List<IotDevice>();

        public void ChangePinStatus(int deviceId, int pinStatus)
        {
            var status = devices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (status != null)
                status.SetPinStatus = pinStatus;
        }

        public IEnumerable<IotDevice> GetAllDevices()
        {
            return devices;
        }

        public IotDevice? GetDeviceStatus(int? deviceId)
        {
            var deviceStatus = devices.FirstOrDefault(d => d.DeviceId == deviceId);
            return devices.FirstOrDefault(d => d.DeviceId == deviceId);
        }

        public void SetDeviceStatus(IotDevice deviceStatus)
        {
            var status = devices.FirstOrDefault(d => d.DeviceId == deviceStatus.DeviceId);
            if (status == null)
                AddDeviceStatus(deviceStatus);
            else
            {
                status.PinStatus = deviceStatus.PinStatus;
            }
        }

        private void AddDeviceStatus(IotDevice deviceStatus)
        {
            devices.Add(deviceStatus);
        }

        public async Task MessageClient(WebSocket webSocket, IDeviceStatusService _statusService)
        {
            var isConnected = true;
            while (isConnected)
            {
                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
                isConnected = !receiveResult.CloseStatus.HasValue;
                if (isConnected)
                {
                    var message = Encoding.UTF8.GetString(buffer);
                    if (int.Parse(message.Split(":")[2]) == 2)
                        _statusService.ChangePinStatus(int.Parse(message.Split(":")[1]), 0);
                    _statusService.SetDeviceStatus(MessageToDeviceStatus(message));
                    var setPin = _statusService.GetDeviceStatus(int.Parse(message.Split(":")[1]));
                    var respBuffer = Encoding.UTF8.GetBytes($"{setPin!.DeviceName}:{setPin.DeviceId}:{setPin.SetPinStatus}:");
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(respBuffer, 0, receiveResult.Count),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                    _statusService.ChangePinStatus(int.Parse(message.Split(":")[1]), 0);
                }
                else
                {
                    await webSocket.CloseAsync(
                    receiveResult.CloseStatus!.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
                }
            }
        }

        public IotDevice MessageToDeviceStatus(string message)
        {
            return new IotDevice
            {
                DeviceId = int.Parse(message.Split(":")[1]),
                DeviceName = message.Split(":")[0],
                PinStatus = int.Parse(message.Split(":")[2]),
            };
        }
    }
}
