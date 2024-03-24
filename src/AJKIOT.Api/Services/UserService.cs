using AJKIOT.Api.Models;
using AJKIOT.Shared.Enums;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AJKIOT.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, ITokenService tokenService, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
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
    }
}
