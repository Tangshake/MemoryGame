namespace LoginService.Entity
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expire { get; set; }
    }
}
