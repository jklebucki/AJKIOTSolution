using AJKIOT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Principal;

namespace AJKIOT.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserCredentials? Credentials { get; set; }
    }

}
