using AJKIOT.Shared.Models;
using System.Net.WebSockets;
using System.Text;

namespace AJKIOT.Api.Services
{
    public class DeviceStatusService : IDeviceStatusService
    {
        private IList<IotDevice> devices = new List<IotDevice>();
        private readonly ILogger<DeviceStatusService> _logger;

        public DeviceStatusService(ILogger<DeviceStatusService> logger)
        {
            _logger = logger;
        }

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

        public async Task MessageClient(WebSocket webSocket, IDeviceStatusService _statusService, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    var parts = message.Split(':');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int deviceId) && int.TryParse(parts[2], out int pinStatus))
                    {
                        if (pinStatus == 2)
                        {
                            _statusService.ChangePinStatus(deviceId, 0);
                        }
                        var deviceStatus = new IotDevice
                        {
                            DeviceId = deviceId,
                            DeviceName = parts[0],
                            PinStatus = pinStatus,
                        };
                        _statusService.SetDeviceStatus(deviceStatus);
                        var setPin = _statusService.GetDeviceStatus(deviceId);
                        var respBuffer = Encoding.UTF8.GetBytes($"{setPin!.DeviceName}:{setPin.DeviceId}:{setPin.SetPinStatus}:");
                        await webSocket.SendAsync(new ArraySegment<byte>(respBuffer, 0, respBuffer.Length), receiveResult.MessageType, receiveResult.EndOfMessage, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing WebSocket message");
                if (webSocket.State != WebSocketState.Closed)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "An error occurred", cancellationToken);
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
