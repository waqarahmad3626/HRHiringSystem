using HRHiringSystem.Domain.Constants;
using HRHiringSystem.Domain.Entities;
using FluentValidation;

namespace HRHiringSystem.Application.Validators;

public class JobValidator : AbstractValidator<Job>
{
    public JobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage(ValidationMessages.Job.TitleRequired)
            .WithErrorCode("JOB001");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(ValidationMessages.Job.DescriptionRequired)
            .WithErrorCode("JOB002");

        RuleFor(x => x.Requirements)
            .NotEmpty()
            .WithMessage(ValidationMessages.Job.RequirementsRequired)
            .WithErrorCode("JOB003");

        RuleFor(x => x.CreatedByHrId)
            .NotEmpty()
            .WithMessage(ValidationMessages.Job.CreatedByRequired)
            .WithErrorCode("JOB004");
    }
}