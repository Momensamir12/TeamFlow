using App.Application.Dto;
using FluentValidation;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator ()
    {
        RuleFor(x => x.ConfirmPassword)
        .SetValidator(new PasswordValidator());

        RuleFor(x => x.NewPassword)
        .SetValidator(new PasswordValidator());

        RuleFor(x => x.NewPassword)
        .Matches(x => x.ConfirmPassword);
    }
}