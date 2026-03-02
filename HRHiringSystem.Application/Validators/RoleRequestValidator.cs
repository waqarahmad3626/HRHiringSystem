using FluentValidation;
using HRHiringSystem.Application.Requests;

namespace HRHiringSystem.Application.Validators;

public class RoleRequestValidator : AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.RoleNameRequired)
            .MaximumLength(200).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.RoleNameTooLong);

        RuleFor(x => x.RoleDescription)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.RoleDescriptionRequired)
            .MaximumLength(500).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.RoleDescriptionTooLong);
    }
}
