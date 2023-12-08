using LoginService.Authentication.Basic.Attributes;
using LoginService.AuthTokens;
using LoginService.DatabaseAccess;
using LoginService.Model;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginService.Controllers;

[ApiController]
[Route("api")]
public class LoginController(ITokenRepository tokenRepository, ITokenManager tokenManager, IUserRepository userRepository) : Controller
{
    [HttpPost]
    [BasicAuthorization]
    [Route("login")]
    public async Task<IResult> LoginUserAsync()
    {
        // Extract user email from header - we know it exists and has been validated
        var authorizationHeader = Request.Headers["Authorization"].ToString();
        var authorization64Decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    authorizationHeader.Replace("Basic ", "", StringComparison.OrdinalIgnoreCase)
                    )
                );

        var authorizationUserEmail = authorization64Decoded.Split(new[] { ':' }, 2)[0];

        // Get user from database
        var user = await userRepository.GetUserByEmailAsync(authorizationUserEmail);

        // Get Jwt secret key from database
        var key = await tokenRepository.GetJwtSecKeyAsync();

        // Generate Jwt token
        var jwtToken = tokenManager.CreateJwtToken(key, new List<Claim> { new Claim(JwtRegisteredClaimNames.Name, authorizationUserEmail) });

        // Generate Refresh token. Token lifetime set in appsettings.json
        var refreshToken = tokenManager.CreateRefreshToken(16);

        // Save refresh token to the database
        var result = tokenRepository.CreateRefreshTokenAsync(user.Id, refreshToken);

        // Construct return object
        var loginUserResponse = new LoginUserResponse { Email = user.Name, Name = user.Name, JwtToken = jwtToken, RefreshToken = refreshToken.Token, Created = refreshToken.Created, Expired = refreshToken.Expired, Success = true };

        return Results.Ok(loginUserResponse);
    }
}
