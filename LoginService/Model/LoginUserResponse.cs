using LoginService.AuthTokens.Model;
using System.ComponentModel.DataAnnotations;

namespace LoginService.Model
{
    public class LoginUserResponse
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string JwtToken { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime Created { get; set; }
        public required DateTime Expired { get; set; }

        public required bool Success { get; set; } = false;
    }
}
