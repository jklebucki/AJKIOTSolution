using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJKIOT.Shared.Models
{
    public class ResetPasswordCustomRequest
    {
        public string Email { get; set; } = string.Empty;
        public string ApplicationAddress { get; set; } = string.Empty;
    }
}
