using HRHiringSystem.Domain.Constants;
using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Validators;

public class CandidateValidator : AbstractValidator<Candidate>
{
    public CandidateValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Candidate.FirstNameRequired)
            .WithErrorCode("CND001");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Candidate.LastNameRequired)
            .WithErrorCode("CND002");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.Candidate.EmailRequired)
            .WithErrorCode("CND003")
            .EmailAddress()
            .WithMessage(ValidationMessages.Candidate.EmailInvalid)
            .WithErrorCode("CND004");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage(ValidationMessages.Candidate.PhoneRequired)
            .WithErrorCode("CND005");
    }
}