using AJKIOT.Api.Hubs;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using AJKIOT.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        public DevicesController(ILogger<DevicesController> logger, IUserService userService,
                                IIotDeviceService iotDeviceService, IHubContext<NotificationHub> notificationHub,
                                ConnectionMapping connectionMapping)
        {
            _logger = logger;
            _userService = userService;
            _iotDeviceService = iotDeviceService;
            _hubContext = notificationHub;
            _connectionMapping = connectionMapping;
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
                await InformClients(updateDeviceRequest.Device.Id);
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
                var apiResponse = await _iotDeviceService.DeleteDeviceAsync(id);
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                var apiResponse = new ApiResponse<bool> { Data = false, Errors = new List<string> { ex.Message } };
                return BadRequest(apiResponse);
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
    }
}
