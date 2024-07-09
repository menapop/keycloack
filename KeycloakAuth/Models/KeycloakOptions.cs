namespace KeycloakAuth.Models
{
    public class KeycloakOptions
    {
        public string AuthUrl { get; set; }
        public string Realm { get; set; }
        public string Audience { get; set; }
        public bool RequireHttpsMetadata { get; set; }
        public string RoleClaimType { get; set; }
        public string NameClaimType { get; set; }

        public TimeSpan? ClockSkew { get; set; }
        public bool SaveToken { get; set; }

        public Credentials Credentials { get; set; }

    }

    public class Credentials
    {
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
    }
}
