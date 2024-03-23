using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AJKIOT.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            var result = await _userService.RegisterUserAsync(request);
            if (result.IsSuccess)
            {
                return CreatedAtAction("Register", result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            var result = await _userService.AuthenticateUserAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(result.Errors);
        }
    }
}
