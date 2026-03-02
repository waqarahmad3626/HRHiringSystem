using FluentValidation;
using HRHiringSystem.Application.Requests;

namespace HRHiringSystem.Application.Validators;

public class JobRequestValidator : AbstractValidator<JobRequest>
{
    public JobRequestValidator()
    {
        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.JobTitleRequired)
            .MaximumLength(150).WithMessage(HRHiringSystem.Application.Constants.RequestConstants.JobTitleTooLong);

        RuleFor(x => x.JobDescription)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.JobDescriptionRequired);

        RuleFor(x => x.JobRequirements)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.JobRequirementsRequired);

        RuleFor(x => x.JobCreatedByHrId)
            .NotEmpty().WithMessage(HRHiringSystem.Application.Constants.RequestConstants.JobCreatedByHrIdRequired);
    }
}
