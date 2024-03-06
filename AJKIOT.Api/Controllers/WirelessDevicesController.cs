using AJKIOT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AJKIoTServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WirelessDevicesController : ControllerBase
    {
        private readonly IDeviceStatusService _statusService;

        public WirelessDevicesController(IDeviceStatusService statusService)
        {
            _statusService = statusService;
        }

        [HttpGet("/ws")]
        public async Task GetAsync()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _statusService.MessageClient(webSocket, _statusService);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [HttpGet("/device/{deviceId}/{status}")]
        public async Task<IActionResult> SetAsync(int deviceId, int status)
        {
            _statusService.ChangePinStatus(deviceId, status);
            return await Task.FromResult(Ok(_statusService.GetAllDevices()));
        }

        [HttpGet("/device/{deviceId}")]
        public async Task<IActionResult> GetStatusAsync(int deviceId)
        {
            return await Task.FromResult(Ok(_statusService.GetDeviceStatus(deviceId)));
        }

        [HttpGet("/device/all")]
        public async Task<IActionResult> GetAllAsync()
        {
            return await Task.FromResult(Ok(_statusService.GetAllDevices()));
        }
    }
}
