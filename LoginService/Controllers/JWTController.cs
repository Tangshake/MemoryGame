using LoginService.Model;
using Microsoft.AspNetCore.Mvc;

namespace LoginService.Controllers
{
    [ApiController]
    [Route("api")]
    public class JwtController() : Controller
    {

        [Route("refresh/jwt")]
        public async Task<IActionResult> RefreshJwtAsync(RefreshTokenRequest refreshTokenRequest)
        {
            return null;
        }
    }
}
