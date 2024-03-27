using AJKIOT.Api.Models;
using AJKIOT.Shared.Enums;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;

namespace AJKIOT.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailSender _emailSenderService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<ApplicationUser> userManager, ITokenService tokenService, ILogger<UserService> logger, IEmailSender emailSenderService, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _emailSenderService = emailSenderService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<AuthResponse>> AuthenticateUserAsync(AuthRequest request)
        {
            var response = new ApiResponse<AuthResponse>();
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
            {
                response.Errors.Add("Unauthorized");
                _logger.LogError($"Unauthorized user: {request.Email}");
                return response;
            }
            _logger.LogInformation($"User authenticated: {request.Email}");
            return await CreateAuthResponseAsync(user.Email!);
        }

        public async Task<ApiResponse<AuthResponse>> RegisterUserAsync(RegistrationRequest request)
        {

            var result = await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                Role = Role.User
            }, request.Password!);

            if (!result.Succeeded)
            {
                var response = new ApiResponse<AuthResponse>();
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
                _logger.LogError($"Error creating user: {string.Join(", ", result.Errors)}");
                return response;
            }
            else
            {
                await _emailSenderService.SendWelcomeEmailAsync(request.Username!, request.Email!);
                var response = await CreateAuthResponseAsync(request.Email!);
                _logger.LogInformation($"User created: {request.Email}");
                return response;
            }

        }
        private async Task<ApiResponse<AuthResponse>> CreateAuthResponseAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var tokens = _tokenService.CreateToken(user!);
            return new ApiResponse<AuthResponse>
            {
                Data = new AuthResponse
                {
                    Username = user!.UserName,
                    Email = user.Email,
                    Tokens = tokens
                },
                Errors = new List<string>()
            };
        }

        public async Task<ApiResponse<bool>> SendPasswordResetLinkAsync(ResetPasswordRequest request)
        {
            var response = new ApiResponse<bool>();
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Attempt to send password reset link for non-existent email: {request.Email}");
                return response;
            }

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var requestScheme = _httpContextAccessor.HttpContext.Request.Scheme;
                var host = _httpContextAccessor.HttpContext.Request.Host.Value;
                var resetLink = $"{requestScheme}://{host}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
                await _emailSenderService.SendPasswordResetEmailAsync(user.Email, user.UserName, resetLink);
                _logger.LogInformation($"Password reset email sent: {request.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while sending the password reset link to {request.Email}: {ex.Message}");
                response.Errors.Add("An error occurred while sending the password reset link.");
            }

            return response;
        }

        public async Task<ApiResponse<bool>> ResetPasswordConfirmAsync(ResetPasswordConfirmRequest request)
        {
            var response = new ApiResponse<bool>();
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Attempt to reset password for non-existent email: {request.Email}");
                return response;
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError($"Error resetting password for {request.Email}: {error.Description}");
                    response.Errors.Add(error.Description);
                }
                return response;
            }

            _logger.LogInformation($"Password has been successfully reset for {request.Email}");
            return response;
        }
    }
}
