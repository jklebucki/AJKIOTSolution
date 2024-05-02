using AJKIOT.Api.Hubs;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using AJKIOT.Shared.Requests;
using AJKIOT.Shared.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IUserService _userService;
        private readonly IIotDeviceService _iotDeviceService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ConnectionMapping _connectionMapping;

        private readonly MqttController _mqttController;

        public DevicesController(ILogger<DevicesController> logger, IUserService userService,
                                IIotDeviceService iotDeviceService, IHubContext<NotificationHub> notificationHub,
                                ConnectionMapping connectionMapping, MqttController mqttController)
        {
            _logger = logger;
            _userService = userService;
            _iotDeviceService = iotDeviceService;
            _hubContext = notificationHub;
            _connectionMapping = connectionMapping;
            _mqttController = mqttController;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserDevicesAsync(string username)
        {
            try
            {
                string ownerId = await _userService.GetUserIdAsync(username);
                var apiResponse = await _iotDeviceService.GetUserDevicesAsync(ownerId);
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return BadRequest(new ApiResponse<List<IotDevice>> { Data = new List<IotDevice>(), Errors = new List<string> { ex.Message } });
            }

        }

        [HttpPost("createDevice")]
        public async Task<IActionResult> AddUserDeviceAsync([FromBody] CreateDeviceRequest createDeviceRequest)
        {
            try
            {
                string ownerId = await _userService.GetUserIdAsync(createDeviceRequest.UserEmail);
                if (ownerId == null)
                    throw new Exception("User not found");
                createDeviceRequest.Device.OwnerId = ownerId;
                var apiResponse = await _iotDeviceService.AddDeviceAsync(createDeviceRequest.Device);
                await InformClients(apiResponse.Data!.Id);
                return Created($"/api/Devices/{apiResponse.Data!.Id}", apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                var apiResponse = new ApiResponse<IotDevice> { Data = createDeviceRequest.Device, Errors = new List<string> { ex.Message } };
                return BadRequest(apiResponse);
            }

        }

        [HttpPut("updateDevice/{id:int}")]
        public async Task<IActionResult> UpdateUserDeviceAsync(int id, [FromBody] UpdateDeviceRequest updateDeviceRequest)
        {
            try
            {
                var device = await _iotDeviceService.GetDeviceAsync(id);
                if (device == null)
                    throw new Exception("Device not found");
                var apiResponse = await _iotDeviceService.UpdateDeviceAsync(updateDeviceRequest.Device);
                // SignalR
                await InformClients(updateDeviceRequest.Device.Id);
                // MQTT features
                foreach (var featureItem in updateDeviceRequest.Device.GetFeatures())
                    await InformDevicesAsync(new MqttMessage
                    {
                        DeviceId = updateDeviceRequest.Device.Id,
                        MessageType = MqttMessageType.UpdateFeature,
                        Message = JsonSerializer.Serialize(featureItem)
                    });
                // MQTT schedule
                foreach (var scheduleItem in updateDeviceRequest.Device.GetSchedule())
                    await InformDevicesAsync(new MqttMessage
                    {
                        DeviceId = updateDeviceRequest.Device.Id,
                        MessageType = MqttMessageType.UpdateSchedule,
                        Message = JsonSerializer.Serialize(scheduleItem)
                    });
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                var apiResponse = new ApiResponse<IotDevice> { Data = updateDeviceRequest.Device, Errors = new List<string> { ex.Message } };
                return BadRequest(apiResponse);
            }

        }

        [HttpDelete("deleteDevice/{id:int}")]
        public async Task<IActionResult> DeleteUserDeviceAsync(int id)
        {
            try
            {
                var device = await _iotDeviceService.GetDeviceAsync(id);
                if (device == null)
                    throw new Exception("Device not found");
                var apiResponse = await _iotDeviceService.DeleteDeviceAsync(id);
                device.Id = -1;//To inform clients that the device was deleted
                await InformClientsDeviceDeleted(device);
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                var apiResponse = new ApiResponse<bool> { Data = false, Errors = new List<string> { ex.Message } };
                return BadRequest(apiResponse);
            }

        }
        [HttpPost("deviceFirmware")]
        public async Task<IActionResult> SendDeviceFirmware([FromBody] ReceiveDeviceFirmwareRequest receiveDeviceFirmwareRequest)
        {
            try
            {
                var stream = await _iotDeviceService.GetDeviceFirmwareAsStreamAsync(new FirmwareSettings
                {
                    WiFiPassword = receiveDeviceFirmwareRequest.WiFiPassword,
                    DeviceId = receiveDeviceFirmwareRequest.DeviceId,
                    WiFiSSID = receiveDeviceFirmwareRequest.WiFiSSID,
                    ApiUrl = receiveDeviceFirmwareRequest.ApiUrl,
                    DeviceType = receiveDeviceFirmwareRequest.DeviceType,
                });
                return File(stream, "application/octet-stream", $"firmware_device_{receiveDeviceFirmwareRequest.DeviceId}.zip");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return BadRequest(ex.Message);
            }
        }

        private async Task<IActionResult> InformClients(int deviceId)
        {
            try
            {
                var device = await _iotDeviceService.GetDeviceAsync(deviceId);
                await _iotDeviceService.UpdateDeviceAsync(device);
                string ownerId = device.OwnerId;
                foreach (var connection in _connectionMapping.GetAllClients().Where(c => c.Value == ownerId))
                {
                    await _hubContext.Clients.Client(connection.Key).SendAsync("DeviceUpdated", device);
                }
                await _hubContext.Clients.All.SendAsync("DeviceUpdated", device);

                var response = new ApiResponse<IotDevice> { Data = device };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return BadRequest(new ApiResponse<IotDevice> { Data = new IotDevice(), Errors = new List<string> { ex.Message } });
            }
        }

        [HttpPost("mqtt")]
        public async Task<IActionResult> PublishMessage([FromBody] MqttMessage mqttMessage)
        {
            if (mqttMessage != null)
            {
                await InformDevicesAsync(mqttMessage);
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        private async Task<IActionResult> InformClientsDeviceDeleted(IotDevice device)
        {
            try
            {
                string ownerId = device.OwnerId;
                foreach (var connection in _connectionMapping.GetAllClients().Where(c => c.Value == ownerId))
                {
                    await _hubContext.Clients.Client(connection.Key).SendAsync("DeviceUpdated", device);
                }
                await _hubContext.Clients.All.SendAsync("DeviceUpdated", device);

                var response = new ApiResponse<IotDevice> { Data = device };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return BadRequest(new ApiResponse<IotDevice> { Data = new IotDevice(), Errors = new List<string> { ex.Message } });
            }
        }

        private async Task InformDevicesAsync(MqttMessage mqttMessage)
        {
            var topic = string.Empty;
            switch (mqttMessage.MessageType)
            {
                case MqttMessageType.Config:
                    topic = $"configDevice/{mqttMessage.DeviceId}";
                    break;
                case MqttMessageType.UpdateFeature:
                    topic = $"updateFeature/{mqttMessage.DeviceId}";
                    break;
                case MqttMessageType.UpdateSchedule:
                    topic = $"configSchedule/{mqttMessage.DeviceId}";
                    break;
                case MqttMessageType.Control:
                    topic = $"controlDevice/{mqttMessage.DeviceId}";
                    break;
                default:
                    break;
            }
            await _mqttController.PublishMessageAsync(topic, mqttMessage.Message);
        }

    }
}
