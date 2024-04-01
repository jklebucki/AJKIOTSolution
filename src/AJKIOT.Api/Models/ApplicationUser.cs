using AJKIOT.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace AJKIOT.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public Role Role { get; set; }
    }
}
