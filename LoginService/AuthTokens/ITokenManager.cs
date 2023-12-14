using LoginService.Entity;
using System.Security.Claims;

namespace LoginService.AuthTokens;

public interface ITokenManager
{
    string CreateJwtToken(string key, IEnumerable<Claim> claims);

    RefreshToken CreateRefreshToken(int size);
}
