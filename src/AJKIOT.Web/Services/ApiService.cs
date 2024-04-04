using AJKIOT.Shared.Models;
using System.Net.Http.Json;

namespace AJKIOT.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, ITokenService tokenService, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<IEnumerable<IotDevice>> GetDeviceStatusAsync(string userId)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Devices/{userId}");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<IEnumerable<IotDevice>>();
                return result ?? new List<IotDevice>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode}");
                return new List<IotDevice>();
            }

        }
    }
}
