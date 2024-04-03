using AJKIOT.Api.Data;
using AJKIOT.Api.Middleware;
using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMessageBus _messageBus;
        private readonly IUserService _userService;

        public DevicesController(IDocumentRepository documentRepository, ILogger<DevicesController> logger, IMessageBus messageBus, IUserService userService)
        {
            _documentRepository = documentRepository;
            _logger = logger;
            _messageBus = messageBus;
            _userService = userService;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetDevices(string username)
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

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageToClient([FromBody] JsonElement message)
        {
            var content = JsonSerializer.Deserialize<JsonElement>(message).ToString();
            try
            {
                _messageBus.EnqueueMessage(content);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
            }
            return Ok();
        }
    }
}
