using HRHiringSystem.Domain.Constants;
using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Validators;

public class RoleValidator : AbstractValidator<Role>
{
    public RoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(ValidationMessages.Role.NameRequired)
            .WithErrorCode("ROL001");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(ValidationMessages.Role.DescriptionRequired)
            .WithErrorCode("ROL002");
    }
}
