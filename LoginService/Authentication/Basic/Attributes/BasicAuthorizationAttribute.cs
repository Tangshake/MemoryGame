using Microsoft.AspNetCore.Authorization;

namespace LoginService.Authentication.Basic.Attributes
{
    public class BasicAuthorizationAttribute : AuthorizeAttribute
    {
        public BasicAuthorizationAttribute()
        {
            AuthenticationSchemes = BasicAuthenticationDefaults.AutenticationScheme;
        }
    }
}
