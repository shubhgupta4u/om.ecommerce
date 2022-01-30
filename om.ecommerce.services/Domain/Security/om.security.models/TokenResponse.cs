using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace om.security.models
{
    public class TokenResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
