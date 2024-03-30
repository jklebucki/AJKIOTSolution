using AJKIOT.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly ILogger<DevicesController> _logger;
        private readonly IDocumentRepository _documentRepository;

        public DevicesController(IDocumentRepository documentRepository, ILogger<DevicesController> logger)
        {
            _documentRepository = documentRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> TestMongoDb()
        {
            try
            {
                return Ok(await _documentRepository.GetAllAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }
    }
}
