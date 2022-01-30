using System;
using System.Collections.Generic;

namespace om.security.models
{
    public class User
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public bool IsMobileVerfied { get; set; }
        public bool IsEmailVerfied { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
    }
}
