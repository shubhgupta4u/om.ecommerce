namespace om.shared.security.models
{
    public class AuthSettings
    {
        public JwtSetting JwtSetting { get; set; }
        public OktaSetting OktaSetting { get; set; }
        public AzureAdSetting AzureAdSetting { get; set; }
    }
}
