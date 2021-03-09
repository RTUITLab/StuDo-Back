namespace Models5.Options
{
    public class JwtOptions
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string RefreshTokenLifeTime { get; set; }
        public string AccessTokenLifeTime { get; set; }
    }
}
