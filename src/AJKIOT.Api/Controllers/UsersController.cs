using AJKIOT.Api.Services;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var apiResponse = new ApiResponse<RegistrationRequest>
                {
                    Data = request
                };
                apiResponse.Errors.AddRange(ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)));
                return BadRequest(apiResponse);
            }
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
            if (!ModelState.IsValid)
            {
                var apiResponse = new ApiResponse<AuthRequest>
                {
                    Data = request
                };
                apiResponse.Errors.AddRange(ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)));
                return BadRequest(apiResponse);
            }
            var result = await _userService.AuthenticateUserAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(result.Errors);
        }

        [HttpPost("reset-password-request")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordCustomRequest model)
        {
            if (!ModelState.IsValid)
            {
                var apiResponse = new ApiResponse<bool>
                {
                    Data = false
                };
                apiResponse.Errors.AddRange(ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)));
                return BadRequest(apiResponse);
            }
            var result = await _userService.SendPasswordResetLinkAsync(model);
            return Ok(result);
        }


        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordConfirmRequest model)
        {
            if (!ModelState.IsValid)
            {
                var apiResponse = new ApiResponse<bool>
                {
                    Data = false
                };
                apiResponse.Errors.AddRange(ModelState.Values.SelectMany(x => x.Errors.Select(y => y.ErrorMessage)));
                return BadRequest(apiResponse);
            }

            var response = await _userService.ResetPasswordConfirmAsync(model);

            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [Authorize]
        [HttpDelete("delete/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var userId = await _userService.GetUserIdAsync(email);
            await _userService.DeleteUserAsync(userId);
            return Ok();
        }
    }
}
