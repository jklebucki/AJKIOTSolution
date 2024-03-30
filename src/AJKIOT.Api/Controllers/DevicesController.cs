using AJKIOT.Api.Middleware;
using AJKIOT.Api.Repositories;
using AJKIOT.Api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDocumentRepository _documentRepository;
        private readonly IMessageBus _messageBus;

        public DevicesController(IDocumentRepository documentRepository, ILogger<DevicesController> logger, IMessageBus messageBus)
        {
            _documentRepository = documentRepository;
            _logger = logger;
            _messageBus = messageBus;
        }

        [HttpGet]
        public async Task<IActionResult> TestMongoDb()
        {
            try
            {
                var result = (await _documentRepository.GetAllAsync()).ToJson();
                return Ok(result);
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
            } catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
            }
            return Ok();
        }
    }
}
