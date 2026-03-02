using FluentValidation;
using HRHiringSystem.Application.Requests;

namespace HRHiringSystem.Application.Validators;

public class UserRequestValidator : AbstractValidator<UserRequest>
{
    public UserRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserNameRequired)
            .MaximumLength(100).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserNameTooLong);

        RuleFor(x => x.UserEmail)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserEmailRequired)
            .EmailAddress().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserEmailInvalid)
            .MaximumLength(150).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserEmailTooLong);

        RuleFor(x => x.UserPassword)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserPasswordRequired)
            .MinimumLength(6).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserPasswordTooShort);

        RuleFor(x => x.UserRoleId)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.UserRoleIdRequired);
    }
}
