using LoginService.AuthTokens.Model;
using LoginService.AuthTokens.Settings;
using LoginService.JWT.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoginService.AuthTokens
{
    public class TokenManager(IOptions<JwtBearerSettings> jwtBearerSettingsOptions, IOptions<RefreshTokenSettings> refreshTokenSettingsOptions) : ITokenManager
    {
        public string CreateJwtToken(string secretKey, IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

            var securityTokenHandler = new JwtSecurityTokenHandler();

            var exp = DateTime.UtcNow.AddMinutes(int.Parse(jwtBearerSettingsOptions.Value.ExpiryTime));

            var token = new JwtSecurityToken(
                    issuer: jwtBearerSettingsOptions.Value.Issuer,
                    audience: jwtBearerSettingsOptions.Value.Audience,
                    claims,
                    expires: exp,
                    signingCredentials: credentials
                );

            var tokenString = securityTokenHandler.WriteToken(token);
            Console.WriteLine($"[CreateToken] JwtToken should be created. JwtToken: {tokenString}");

            return tokenString;
        }

        public RefreshToken CreateRefreshToken(int size)
        {
            byte[] byteArray = new byte[size];
            var rng = RandomNumberGenerator.Create();

            rng.GetBytes(byteArray);

            var token = Convert.ToBase64String(byteArray);

            return new RefreshToken { Token = token, Created = DateTime.Now, Expired = DateTime.Now.AddDays(refreshTokenSettingsOptions.Value.ExpiryTime) };
        }

    }
}
