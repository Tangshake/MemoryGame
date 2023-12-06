namespace LoginService.JWT.Settings
{
    public class JwtBearerSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ExpiryTime { get; set; }
    }
}
