using System.Text.Json.Serialization;

namespace AJKIOT.Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        Admin,
        User,
        RefreshToken
    }
}
