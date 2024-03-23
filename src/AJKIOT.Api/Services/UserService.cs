using AJKIOT.Api.Data;
using AJKIOT.Api.Models;
using AJKIOT.Shared.Enums;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AJKIOT.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ITokenService tokenService, IEmailSender emailSender, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<ApiResponse<AuthResponse>> AuthenticateUserAsync(AuthRequest request)
        {
            var response = new ApiResponse<AuthResponse>();
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password!))
            {
                response.Errors.Add("Unauthorized");
                return response;
            }

            return await CreateAuthResponseAsync(user.Email!);
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
                return response;
            }
            else
            {
                //var response = await CreateAuthResponseAsync(request.Email!);
                var user = await _userManager.FindByEmailAsync(request.Email!);
                return new ApiResponse<AuthResponse>
                {
                    Data = new AuthResponse
                    {
                        Username = request.Username,
                        Email = request.Email,
                        Tokens = _tokenService.CreateToken(user!)
                    },
                    Errors = new List<string>()
                };
            }

        }
    }
}
