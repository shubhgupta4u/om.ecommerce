namespace om.shared.security.models
{
    public class JwtSetting
    {
        public string[] AllowedOrigins { get; set; }
        public string SecurityKey { get; set; }
        public string Scope { get; set; }
        public int ExpireTime { get; set; }
        public int PreviousTokenValidDuration { get; set; }
    }   
}
