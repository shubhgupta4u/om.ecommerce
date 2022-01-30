using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace om.security.models
{
    public class TokenRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string GrantType { get; set; }
        public string Scope { get; set; }
    }
}
