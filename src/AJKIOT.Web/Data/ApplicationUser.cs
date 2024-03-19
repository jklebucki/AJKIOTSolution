using AJKIOT.Shared.Models;

namespace AJKIOT.Web.Data
{
    public class ApplicationUser
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserCredentials? Credentials { get; set; }
    }

}
