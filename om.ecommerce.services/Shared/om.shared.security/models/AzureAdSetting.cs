namespace om.shared.security.models
{
    public class AzureAdSetting
    {
        public string TenantId { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string SecretKey { get; set; }
    }
}
