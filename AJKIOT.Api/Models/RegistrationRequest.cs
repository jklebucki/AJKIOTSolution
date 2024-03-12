using AJKIOT.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Api.Models
{
    public class RegistrationRequest
    {
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        public Role Role { get; set; }
    }
}
