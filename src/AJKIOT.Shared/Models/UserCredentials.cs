using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AJKIOT.Shared.Models
{
    public class UserCredentials
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
