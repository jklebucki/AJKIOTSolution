using AJKIOT.Shared.Models;
using AJKIOT.Shared.Requests;
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

        public string ApiUrl() => _httpClient.BaseAddress!.ToString();
        public async Task<ApiResponse<IotDevice>> CreateUserDeviceAsync(CreateDeviceRequest device)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/Devices/createDevice");
            request.Content = JsonContent.Create(device);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IotDevice>>();
                return result!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} - {response.ReasonPhrase}");
                return new ApiResponse<IotDevice>() { Data = device.Device, Errors = new List<string>() { response.ReasonPhrase! } };
            }

        }

        public async Task<ApiResponse<bool>> DeleteDeviceAsync(int deviceId)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/Devices/deleteDevice/{deviceId}");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} - {response.ReasonPhrase}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result!;
            }
        }

        public async Task<ApiResponse<IEnumerable<IotDevice>>> GetUserDevicesAsync(string userId)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/Devices/{userId}");
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<IotDevice>>>();
                return result!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} - {response.ReasonPhrase}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<IotDevice>>>();
                return result!;
            }

        }

        public async Task<ApiResponse<IotDevice>> UpdateDeviceAsync(UpdateDeviceRequest updateDeviceRequest)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/Devices/updateDevice/{updateDeviceRequest.Device.Id}");
            request.Content = JsonContent.Create(updateDeviceRequest);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<IotDevice>>();
                return result!;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} - {response.ReasonPhrase}");
                return new ApiResponse<IotDevice>() { Data = updateDeviceRequest.Device, Errors = new List<string>() { response.ReasonPhrase! } };
            }
        }

        public async Task<ApiResponse<Stream>> ReceiveDeviceFirmwareAsync(ReceiveDeviceFirmwareRequest receiveDeviceFirmwareRequest)
        {
            await _tokenService.AddTokenToHeader(_httpClient);
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/Devices/deviceFirmware");
            request.Content = JsonContent.Create(receiveDeviceFirmwareRequest);
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return new ApiResponse<Stream> { Data = stream, Errors = new List<string>() };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode} - {response.ReasonPhrase}");
                return new ApiResponse<Stream>() { Data = Stream.Null, Errors = new List<string>() { response.ReasonPhrase! } };
            }
        }

    }
}
