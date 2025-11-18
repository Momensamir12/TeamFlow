using System.Data;
using App.Application.Dto;
using FluentValidation;

public class PasswordResetValidator : AbstractValidator<ResetPasswordDto>
{
    public PasswordResetValidator ()
    {
        RuleFor(x => x.ConfirmPassword)
        .SetValidator(new PasswordValidator());

        RuleFor(x => x.NewPassword)
        .SetValidator(new PasswordValidator());

        RuleFor(x => x.NewPassword)
        .Matches(x => x.ConfirmPassword);
    }
}