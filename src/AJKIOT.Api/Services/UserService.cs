﻿using AJKIOT.Api.Events;
using AJKIOT.Api.Models;
using AJKIOT.Shared.Enums;
using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace AJKIOT.Api.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public event EventHandler<UserEventArgs>? UserDeleted;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IEmailSender _emailSenderService;
        private readonly IIotDeviceService _iotDeviceService;

        public UserService(UserManager<ApplicationUser> userManager, ITokenService tokenService, ILogger<UserService> logger,
                            IEmailSender emailSenderService, IIotDeviceService iotDeviceService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
            _emailSenderService = emailSenderService;
            _iotDeviceService = iotDeviceService;
            UserDeleted += UserEvents.UserService_UserDeleted!;
        }

        protected virtual void OnUserDeleted(ApplicationUser user)
        {
            UserEventArgs args = new UserEventArgs { User = user };
            UserDeleted?.Invoke(_iotDeviceService, args);
        }
        public class UserEventArgs : EventArgs
        {
            public ApplicationUser? User { get; set; }
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
                FullName = request.Username!,
                UserName = request.Email,
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
                await _emailSenderService.SendWelcomeEmailAsync(request.Username!, request.Email!, request.ApplicationAddress);
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
                    Username = user!.FullName,
                    Email = user.Email,
                    Tokens = tokens
                },
                Errors = new List<string>()
            };
        }

        public async Task<ApiResponse<bool>> SendPasswordResetLinkAsync(ResetPasswordCustomRequest request)
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
                var resetLink = $"{request.ApplicationAddress}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";
                await _emailSenderService.SendResetPasswordEmailAsync(user.Email!, user.FullName, resetLink);
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
                response.Errors.Add("Invalid email address.");
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

            await _emailSenderService.SendResetPasswordConfirmationEmailAsync(user.Email, user.FullName, request.ApplicationAddress);
            return response;
        }

        public async Task<string> GetUserIdAsync(string username)
        {
            var user = await _userManager.FindByEmailAsync(username);
            if (user != null)
                return user.Id;
            return string.Empty;
        }

        public async Task<string> GetUsernameAsync(string ownerId)
        {
            var user = await _userManager.FindByIdAsync(ownerId);
            if (user != null)
                return user.UserName!;
            return string.Empty;
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    OnUserDeleted(user);
                }
                else
                {
                    throw new InvalidOperationException("Failed to delete user.");
                }
            }
        }
    }
}
