using System;

namespace om.shared.caching.Models
{
    public class UserTokenInfo
    {
        public string UserId { get; set; }
        public string TokenId { get; set; }
        public string CurrentToken { get; set; }
        public string PreviousToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsActive { get; set; }
        public string RemoteIpAddress { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
    }
}
