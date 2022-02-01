namespace om.security.models
{
    public enum GrantType
    {
        Password=1,
        Okta=2,
        AzureAD=3
    }
    public class TokenRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public GrantType GrantType { get; set; }
        public string BearerToken { get; set; }
        public string Scope { get; set; }
    }
}
