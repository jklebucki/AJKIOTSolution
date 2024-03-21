using AJKIOT.Shared.Models;
using System.Net.Http.Json;

namespace AJKIOT.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;

        public ApiService(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;

        }

        public async Task<IEnumerable<DeviceStatus>> GetDeviceStatusAsync(int userId)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IEnumerable<DeviceStatus>>("device/all");
                if (response != null)
                {
                    return response;
                }
                return new List<DeviceStatus>();
            }
            catch (Exception ex)
            {

                return new List<DeviceStatus>();
            }

        }
    }
}
