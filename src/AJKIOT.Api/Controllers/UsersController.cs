using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity.Data;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            var result = await _userService.RegisterUserAsync(request);
            if (result.IsSuccess)
            {
                return CreatedAtAction("Register", result);
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            var result = await _userService.AuthenticateUserAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(result.Errors);
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _userService.SendPasswordResetLinkAsync(model);
            return Ok(result);
        }


        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.ResetPasswordConfirmAsync(model);

            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
