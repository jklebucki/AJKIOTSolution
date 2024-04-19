using AJKIOT.Api.Hubs;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using AJKIOT.Shared.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly NotificationHub _notificationHub;

        public DevicesController(ILogger<DevicesController> logger, IUserService userService, IIotDeviceService iotDeviceService , NotificationHub notificationHub)
        {
            _logger = logger;
            _userService = userService;
            _iotDeviceService = iotDeviceService;
            _notificationHub = notificationHub;
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

        [HttpPost("updateDevice")]
        public async Task<IActionResult> UpdateUserDeviceAsync([FromBody] UpdateDeviceRequest updateDeviceRequest)
        {
            try
            {
                var apiResponse = await _iotDeviceService.UpdateDeviceAsync(updateDeviceRequest.Device);
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

        [HttpGet("SignalR/{deviceId:int}")]
        public async Task<IActionResult> TestSignalR(int deviceId)
        {
            try
            {
                var device = await _iotDeviceService.GetDeviceAsync(deviceId);
                device.DeviceName = $"{device.DeviceName} - {DateTime.Now.ToString()}";
                await _notificationHub.UpdateDevice(device);
                var response = new ApiResponse<IotDevice> { Data = device };
                return Ok(device);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return BadRequest(new ApiResponse<IotDevice> { Data = new IotDevice(), Errors = new List<string> { ex.Message } });
            }

        }
    }
}
