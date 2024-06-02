
using System.Text.Json;

namespace AJKIOT.Web.Settings
{
    public class ApiSettings
    {
        public string ApiBaseUrl { get; private set; } = string.Empty;
        public string ApiScheme { get; private set; } = string.Empty;

        public ApiSettings( string apiBaseUrl, string apiScheme)
        {
            ApiBaseUrl = apiBaseUrl;
            ApiScheme = apiScheme;
        }     
    }
}
