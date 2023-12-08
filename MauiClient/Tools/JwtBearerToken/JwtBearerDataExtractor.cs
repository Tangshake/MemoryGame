using System.IdentityModel.Tokens.Jwt;

namespace MemoryGame.Tools.JwtBearerToken;

public static class JwtBearerDataExtractor
{
    public static string ExctractClaim(string token, string claimType)
    {
        if (!string.IsNullOrEmpty(token) && claimType != null)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var jsonToken = tokenHandler.ReadJwtToken(token);
            var tokenS = jsonToken as JwtSecurityToken;

            var claim = tokenS.Claims.First(claim => claim.Type == claimType);

            if (claim is null)
                return string.Empty;

            return claim.Value;
        }

        return string.Empty;
    }

    public static DateTime GetExpireDate(string token)
    {
        if (string.IsNullOrEmpty(token))
            return DateTime.MinValue;

        var exp = ExctractClaim(token, "exp");

        var expireDate = DateTimeOffset.FromUnixTimeSeconds(int.Parse(exp)).LocalDateTime;

        return expireDate;
    }

    public static bool Expired(string token)
    {
        var expireDate = GetExpireDate(token);

        if (DateTime.Now.ToLocalTime() > expireDate)
            return true;

        return false;
    }
}
