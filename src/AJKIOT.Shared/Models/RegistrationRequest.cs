﻿
using AJKIOT.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Shared.Models
{
    public class RegistrationRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string ApplicationAddress { get; set; } = string.Empty;
        [Required]
        public Role Role { get; set; }
    }
}
