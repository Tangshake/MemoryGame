using FluentValidation;
using RegisterService.DataTransferObject;

namespace RegisterService.FluentValidator
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(reg => reg.Email).EmailAddress();
            RuleFor(reg => reg.Password)
                .NotEmpty()
                .NotNull();
            RuleFor(reg => reg.Name)
                .NotEmpty()
                .NotNull()
                .Length(3, 20);
        }
    }
}
