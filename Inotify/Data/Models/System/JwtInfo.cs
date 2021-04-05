namespace Inotify.Data.Models.System
{
    public class JwtInfo
    {
        public int ClockSkew { get; set; }

        public string Audience { get; set; }

        public string Issuer { get; set; }

        public string IssuerSigningKey { get; set; }

        public int Expiration { get; set; }

    }
}
