using FluentValidation;
using HRHiringSystem.Application.Requests;
using HRHiringSystem.Application.Constants;

namespace HRHiringSystem.Application.Validators;

public class JobApplicationRequestValidator : AbstractValidator<JobApplicationRequest>
{
    private static readonly string[] AllowedFileTypes = new[]
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public JobApplicationRequestValidator()
    {
        RuleFor(x => x.CandidateFirstName)
            .NotEmpty().WithMessage(RequestConstants.CandidateFirstNameRequired)
            .MaximumLength(100).WithMessage(RequestConstants.CandidateFirstNameTooLong);

        RuleFor(x => x.CandidateLastName)
            .NotEmpty().WithMessage(RequestConstants.CandidateLastNameRequired)
            .MaximumLength(100).WithMessage(RequestConstants.CandidateLastNameTooLong);

        RuleFor(x => x.CandidateEmail)
            .NotEmpty().WithMessage(RequestConstants.CandidateEmailRequired)
            .EmailAddress().WithMessage(RequestConstants.CandidateEmailInvalid)
            .MaximumLength(150).WithMessage(RequestConstants.CandidateEmailTooLong);

        RuleFor(x => x.CandidatePhone)
            .NotEmpty().WithMessage(RequestConstants.CandidatePhoneRequired)
            .MaximumLength(20).WithMessage(RequestConstants.CandidatePhoneTooLong);

        RuleFor(x => x.JobId)
            .NotEmpty().WithMessage(RequestConstants.JobApplicationJobIdRequired);

        // CV File validation
        RuleFor(x => x.CvFile)
            .NotNull().WithMessage(RequestConstants.JobApplicationCvUrlRequired)
            .Must(file => file != null && file.Length > 0)
            .WithMessage(RequestConstants.JobApplicationCvUrlRequired)
            .Must(file => file != null && AllowedFileTypes.Contains(file.ContentType))
            .WithMessage(RequestConstants.JobApplicationCvFileInvalidType)
            .Must(file => file != null && file.Length <= MaxFileSize)
            .WithMessage(RequestConstants.JobApplicationCvFileTooLarge);
    }
}
