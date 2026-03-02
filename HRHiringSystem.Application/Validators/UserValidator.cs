using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Validators;

using FluentValidation;
using HRHiringSystem.Domain.Constants;
using HRHiringSystem.Domain.Entities;

public class UserValidator : AbstractValidator<User>
{
        public UserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(ValidationMessages.User.NameRequired)
                .WithErrorCode("USR001");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage(ValidationMessages.User.EmailRequired)
                .WithErrorCode("USR002")
                .EmailAddress()
                .WithMessage(ValidationMessages.User.EmailInvalid)
                .WithErrorCode("USR003");

            RuleFor(x => x.PasswordHash)
                .NotEmpty()
                .WithMessage(ValidationMessages.User.PasswordRequired)
                .WithErrorCode("USR004");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage(ValidationMessages.User.RoleRequired)
                .WithErrorCode("USR005");
        }
    }