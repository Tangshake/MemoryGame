using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;
using RegisterService.DatabaseAccess;
using RegisterService.DataTransferObject;
using RegisterService.PasswordManagement;
using RegisterService.RandomTokenGenerator;

namespace RegisterService.Controllers;

[ApiController]
[Route("api")]
public class RegisterController(IConfiguration configuration,IPasswordHash passwordManager, IEmailService emailService, IRegisterRepository registerRepository, IUserRepository userRepository, IValidator<RegisterRequest> validator) : Controller
{
    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IResult> RegisterUserAsync([FromBody] RegisterRequest registerRequest)
    {
        // Validate received data
        var validationResult = validator.Validate(registerRequest);

        if(!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        // Check if player with received email address already exists
        var result = await userRepository.UserExistsByEmailAsync(registerRequest.Email);

        if (result == -1)
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        else if (result > 0)
            return Results.Conflict(new RegisterResponse { Id = -1, RegisterSuccess = false, Message = "User with provided email already exists" });

        // Hash password before storing it in database
        var hashedPassword = passwordManager.HashPassword(registerRequest.Password, 13);

        // Add database record for a user without activated account
        var userId = await userRepository.CreateUserAsync(registerRequest.Name, registerRequest.Email, hashedPassword);

        if(userId == -1)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        // User should be added. Add registration record for the user
        var token = TokenGenerator.GenerateRandomAlphanumericString();
        var tokenLifetime = configuration.GetValue<int>("Token:Lifetime");

        var registerResult = await registerRepository.SetUserActivationAsync(userId: userId, tokenLifetime, token: token);

        if(registerResult == -1)
            return Results.StatusCode(StatusCodes.Status500InternalServerError);

        await emailService.SendAsync(registerRequest.Email, "Household Spirits registration", $"Here is your code (valid for {tokenLifetime} minutes): {token}", true);

        return Results.Ok(new RegisterResponse { Id = userId, RegisterSuccess = true, Message="OK"});
    }
}
