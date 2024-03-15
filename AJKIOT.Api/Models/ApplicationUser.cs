using AJKIOT.Shared.Enums;
using Microsoft.AspNetCore.Identity;

namespace AJKIOT.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Role Role { get; set; }
    }
}
