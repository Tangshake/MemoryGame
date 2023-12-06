namespace LoginService.AuthTokens.Model
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expired { get; set; }
    }
}
