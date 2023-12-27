using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoryGame.Bearer;

public static class JwtInformationExtractor
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

    public static bool? Expired(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var exp = ExctractClaim(token, "exp");

        var expireDate = DateTimeOffset.FromUnixTimeSeconds(int.Parse(exp)).LocalDateTime;

        if (DateTime.Now.ToLocalTime() > expireDate)
            return true;

        return false;
    }

}