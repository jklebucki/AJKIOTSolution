using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
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

        public DevicesController(ILogger<DevicesController> logger, IUserService userService, IIotDeviceService iotDeviceService)
        {
            _logger = logger;
            _userService = userService;
            _iotDeviceService = iotDeviceService;
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
        public async Task<IActionResult> AddUserDeviceAsync([FromBody] IotDevice device)
        {
            try
            {
                string ownerId = await _userService.GetUserIdAsync(device.OwnerId);
                if (ownerId == null)
                    throw new Exception("User not found");
                device.OwnerId = ownerId;
                var apiResponse = await _iotDeviceService.AddDeviceAsync(device);
                return Created($"/api/Devices/{apiResponse.Data!.Id}", apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                var apiResponse = new ApiResponse<IotDevice> { Data = device, Errors = new List<string> { ex.Message } };
                return BadRequest(apiResponse);
            }

        }
    }
}
