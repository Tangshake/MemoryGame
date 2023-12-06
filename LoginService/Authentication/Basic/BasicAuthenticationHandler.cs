using LoginService.DatabaseAccess;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace LoginService.Authentication.Basic
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository userRepository;

        public BasicAuthenticationHandler(IUserRepository userRepository, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            this.userRepository = userRepository;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing authorization key");

            var authorizationHeader = Request.Headers["Authorization"].ToString();

            if(!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                if (Request.Headers.ContainsKey("Authorization"))
                    return AuthenticateResult.Fail("Authorization header has no basic keyword");
            }

            var authorization64Decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(
                    authorizationHeader.Replace("Basic ", "", StringComparison.OrdinalIgnoreCase)
                    )
                );

            var authorizationSplit = authorization64Decoded.Split(new[] { ':' }, 2);

            if(authorizationSplit.Length != 2)
                return AuthenticateResult.Fail("Authorization header format is invalid");

            var userEmail = authorizationSplit[0];
            var userPassword = authorizationSplit[1];

            // get user from database
            var user = await userRepository.GetUserByEmailAsync(userEmail);

            if(user is null)
                return AuthenticateResult.Fail("Could not validate users creditentials.");

            // Verify user against database
            var result = BCrypt.Net.BCrypt.EnhancedVerify(userPassword, user.Password);

            if (!result)
                return AuthenticateResult.Fail("Authorization failed");

            var userIdentity = new BasicAuthenticationClient
            {
                AuthenticationType = BasicAuthenticationDefaults.AutenticationScheme,
                IsAuthenticated = true,
                Name = userEmail
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(userIdentity, new[]
            {
                new Claim(ClaimTypes.Name, userEmail)
            }));

            return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
        }
    }
}
