using App.Infrastructure.Auth.Entities;
using FluentValidation;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator (){
        RuleFor(x => x)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain a number");
    }
 }   