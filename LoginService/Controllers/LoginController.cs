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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IResult> LoginUserAsync()
    {
        // Extract user email from header - we know it exists and has been validated
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authorizationHeader))
            return Results.BadRequest();

        var authorization64Decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    authorizationHeader.Replace("Basic ", "", StringComparison.OrdinalIgnoreCase)
                    )
                );

        var authorizationUserDetails = authorization64Decoded.Split(new[] { ':' }, 2);

        if(authorizationUserDetails is null || authorizationUserDetails.Length != 2)
            return Results.BadRequest();

        var authorizationUserEmail = authorizationUserDetails[0];

        // Get user from database
        var user = await userRepository.GetUserByEmailAsync(authorizationUserEmail);

        if (user is null)
            return Results.Unauthorized();

        // Get Jwt secret key from database
        var key = await tokenRepository.GetJwtSecKeyAsync();

        if (string.IsNullOrEmpty(key))
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        // Generate Jwt token
        var jwtToken = tokenManager.CreateJwtToken(key, new List<Claim> { new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(JwtRegisteredClaimNames.Name, user.Name) });

        if (string.IsNullOrWhiteSpace(jwtToken))
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        // Generate Refresh token. Token lifetime set in appsettings.json
        var refreshToken = tokenManager.CreateRefreshToken(16);

        if (refreshToken is null)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        // Save refresh token to the database
        var result = tokenRepository.CreateRefreshTokenAsync(user.Id, refreshToken);

        if (result is null)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        // Construct return object
        var loginUserResponse = new LoginUserResponse { Id = user.Id, Email = user.Email, Name = user.Name, JwtToken = jwtToken, RefreshToken = refreshToken.Token, Created = refreshToken.Created, Expired = refreshToken.Expire, Success = true };

        return Results.Ok(loginUserResponse);
    }
}
