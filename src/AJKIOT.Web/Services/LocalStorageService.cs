using AJKIOT.Shared.Models;
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

        public async Task SaveTokenAsync(string accessToken, string refreshToken)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", accessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
        }

        public async Task<UserCredentials> GetTokenAsync()
        {
            var accessToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");
            var refreshToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            return new UserCredentials() { AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task ClearTokenAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "accessToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
        }
    }
}
