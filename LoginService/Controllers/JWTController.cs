using LoginService.Authentication.Basic.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace LoginService.Controllers;

[Route("api/jwt")]
public class JWTController : Controller
{

    //[Route("refresh")]
    //[BasicAuthorization]
    //public async Task<IResult> RefreshJWTTokenAsync()
    //{
    //    return null;
    //}
}
