using LoginService.AuthTokens;
using LoginService.DatabaseAccess;
using LoginService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LoginService.Controllers
{
    [ApiController]
    [Route("api")]
    public class JwtController(ITokenManager tokenManager, ITokenRepository tokenRepository, IUserRepository userRepository) : Controller
    {

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh/jwt")]
        public async Task<IResult> RefreshJwtAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if(refreshTokenRequest is null)
                return Results.Unauthorized();

            try
            {
                // Get users refresh token
                var refreshToken = await tokenRepository.GetRefreshTokenAsync(refreshTokenRequest.UserId);

                // When there is no refresh token for specific user assume unauthorized access
                if (refreshToken is null)
                    return Results.Unauthorized();

                // When refresh tokens do not matches return unauthorized
                if (!refreshTokenRequest.RefreshToken.Equals(refreshToken.Token))
                    return Results.Unauthorized();

                // When refresh token is expired return unauthorized
                if(DateTime.Compare(refreshToken.Expire, DateTime.Now) < 0)
                    return Results.Unauthorized();

                // So far we know both refresh tokens matches and are not expired
                // Extract expired jwt claims and compare it with users data

            }
            catch (Exception)
            {

                throw;
            }

            try
            {
                // Get user from database
                var user = await userRepository.GetUserByIdAsync(refreshTokenRequest.UserId);

                // If there is no user with provided id return Unauthorized
                if(user is null)
                    return Results.Unauthorized();

                // Compare email and name
                var jwtName = JwtInformationExtractor.ExctractClaim(refreshTokenRequest.JwtToken, JwtRegisteredClaimNames.Name);
                var jwtEmail = JwtInformationExtractor.ExctractClaim(refreshTokenRequest.JwtToken, JwtRegisteredClaimNames.Email);

                // Check if claims exists and are not empty
                if(string.IsNullOrEmpty(jwtName) || string.IsNullOrEmpty(jwtEmail))
                    return Results.Unauthorized();

                // Compare
                if(!jwtName.Equals(user.Name) || !jwtEmail.Equals(user.Email))
                    return Results.Unauthorized();

                // So far so good. Generate both tokens and return
                // Get Jwt secret key from database
                var key = await tokenRepository.GetJwtSecKeyAsync();

                // Generate Jwt token
                var jwtToken = tokenManager.CreateJwtToken(key, new List<Claim> { new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(JwtRegisteredClaimNames.Name, user.Name) });

                // Generate Refresh token. Token lifetime set in appsettings.json
                var refreshToken = tokenManager.CreateRefreshToken(16);

                // Save refresh token to the database
                var result = tokenRepository.CreateRefreshTokenAsync(user.Id, refreshToken);

                // Construct return object
                var refreshTokenResponse = new RefreshTokenResponse { JwtToken = jwtToken, RefreshToken = refreshToken.Token };

                return Results.Ok(refreshTokenResponse);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
