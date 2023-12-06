using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegisterService.DatabaseAccess;
using RegisterService.DataTransferObject;
using System.Net;
using System.Net.Http.Headers;

namespace RegisterService.Controllers;

[ApiController]
public class TokenVerificationController(IRegisterRepository registerRepository, IUserRepository userRepository) : Controller
{
    [AllowAnonymous]
    [HttpPost]
    [Route("api/verifytoken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> VerifyTokenAsync(TokenVerifyRequest tokenVerifyRequest)
    {
        if (tokenVerifyRequest.Id < 0 || string.IsNullOrEmpty(tokenVerifyRequest.Token))
            return Results.BadRequest();

        var user = await userRepository.GetUserByIdAsync(tokenVerifyRequest.Id);

        if (user is null)
            return Results.NotFound();

        // Check if account is already active
        if (user.Active == 1)
            return Results.BadRequest();

        // Get user activation data
        var activationData = await registerRepository.GetUserActivationInformationAsync(tokenVerifyRequest.Id);

        // May happen that register time has passed and record was removed
        if(activationData is null)
            return Results.BadRequest();

        // Check if activation time has passed
        var compare = DateTime.Compare(activationData.ActivationEnd, DateTime.Now);

        // When time to activate has passed but entry has not been removed -> Clean
        // Remove not active user account and activation entry
        if (compare > 0)
        {
            var userRemoveResult = await userRepository.RemoveUnactivatedUserAccountAsync(tokenVerifyRequest.Id);
            var activationRemoveResult = await registerRepository.RemoveActivationEntryAsync(tokenVerifyRequest.Id);

            return Results.BadRequest();
        }

        // Check if activation token equals sent token
        if(!activationData.Secret.Equals(tokenVerifyRequest.Token))
            return Results.BadRequest();

        // Activate user account
        var activationResult = await userRepository.ActivateUserAccountAsync(tokenVerifyRequest.Id);
        var activationEntryRemoveResult = await registerRepository.RemoveActivationEntryAsync(tokenVerifyRequest.Id);

        if(activationResult == -1)
            return Results.BadRequest();


        return Results.Ok();
    }

}
