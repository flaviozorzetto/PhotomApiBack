namespace PhotomApi.Config
{
    public class JwtCredentialOptions
    {
        public string? ClientID { get; set; }
        public string? ClientSecret { get; set; }
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }
}
