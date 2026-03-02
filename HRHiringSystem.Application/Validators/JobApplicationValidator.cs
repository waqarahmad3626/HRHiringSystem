using HRHiringSystem.Domain.Constants;
using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Validators;

public class JobApplicationValidator : AbstractValidator<JobApplication>
{
    public JobApplicationValidator()
    {
        RuleFor(x => x.CandidateId)
            .NotEmpty()
            .WithMessage(ValidationMessages.JobApplication.CandidateRequired)
            .WithErrorCode("JAP001");

        RuleFor(x => x.JobId)
            .NotEmpty()
            .WithMessage(ValidationMessages.JobApplication.JobRequired)
            .WithErrorCode("JAP002");

        RuleFor(x => x.CVUrl)
            .NotEmpty()
            .WithMessage(ValidationMessages.JobApplication.CvRequired)
            .WithErrorCode("JAP003");
    }
}