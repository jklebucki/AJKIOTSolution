using AJKIOT.Api.Data;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IMessageBus _messageBus;
        private readonly IUserService _userService;

        public DevicesController(ILogger<DevicesController> logger, IMessageBus messageBus, IUserService userService)
        {
            _logger = logger;
            _messageBus = messageBus;
            _userService = userService;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserDevicesAsync(string username)
        {
            try
            {
                string ownerId = await _userService.GetUserIdAsync(username);
                var userDevices = DevicesTestData.Devices().Where(x => x.OwnerId == ownerId).ToList();
                return Ok(userDevices);
            }
            catch (Exception ex)
            {

                _logger.LogError($"Something went wrong: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPost("adddevice")]
        public async Task<IActionResult> SetDeviceAsync([FromBody] IotDevice device)
        {
            var content = JsonSerializer.Serialize(device);
            try
            {

            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
            }
            return await Task.FromResult<IActionResult>(Ok()).ConfigureAwait(false);
        }
    }
}
