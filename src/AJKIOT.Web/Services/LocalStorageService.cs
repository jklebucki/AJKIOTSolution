using AJKIOT.Shared.Models;
using AJKIOT.Web.Data;
using Microsoft.JSInterop;

namespace AJKIOT.Web.Services
{
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SaveApplicationUserAsync(ApplicationUser user)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userName", user.Username);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "email", user.Email);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", user.Credentials!.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", user.Credentials!.RefreshToken);
        }

        public async Task<ApplicationUser> GetApplicationUserAsync()
        {
            var accessToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            var userName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userName");
            var email = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "email");
            var credentials = new UserCredentials { AccessToken = accessToken, RefreshToken = refreshToken };
            return new ApplicationUser { Username = userName, Email = email, Credentials = credentials };
        }

        public async Task ClearTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userName");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "email");
        }
    }
}
