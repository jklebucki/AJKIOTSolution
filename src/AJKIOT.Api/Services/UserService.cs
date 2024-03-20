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
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ITokenService tokenService, IEmailSender emailSender, ILogger logger)
        {
            _userManager = userManager;
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<ServiceResponse<AuthResponse>> AuthenticateUserAsync(AuthRequest request)
        {
            var response = new ServiceResponse<AuthResponse>();
            var user = await _userManager.FindByEmailAsync(request.Email!);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, request.Password!)))
            {
                response.Errors.Add("Unauthorized");
                return response;
            }

            var token = _tokenService.CreateToken(user);
            response.Data = new AuthResponse
            {
                Username = user.UserName,
                Email = user.Email,
                Token = token,
            };
            return response;
        }

        public async Task<ServiceResponse<RegistrationRequest>> RegisterUserAsync(RegistrationRequest request)
        {
            var response = new ServiceResponse<RegistrationRequest>();
            var result = await _userManager.CreateAsync(new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                Role = Role.User
            }, request.Password!);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
                return response;
            }

            response.Data = request;
            return response;
        }
    }
}
