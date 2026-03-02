namespace HRHiringSystem.Application.Constants;

public static class RequestConstants
{
    // User request messages
    public const string UserNameRequired = "UserName is required.";
    public const string UserNameTooLong = "UserName must be at most 100 characters.";
    public const string UserEmailRequired = "UserEmail is required.";
    public const string UserEmailInvalid = "UserEmail must be a valid email address.";
    public const string UserEmailTooLong = "UserEmail must be at most 150 characters.";
    public const string UserPasswordRequired = "UserPassword is required.";
    public const string UserPasswordTooShort = "UserPassword must be at least 6 characters.";
    public const string UserRoleIdRequired = "UserRoleId is required.";

    // Role request messages
    public const string RoleNameRequired = "RoleName is required.";
    public const string RoleNameTooLong = "RoleName must be at most 200 characters.";
    public const string RoleDescriptionRequired = "RoleDescription is required.";
    public const string RoleDescriptionTooLong = "RoleDescription must be at most 500 characters.";

    // Job request messages
    public const string JobTitleRequired = "JobTitle is required.";
    public const string JobTitleTooLong = "JobTitle must be at most 150 characters.";
    public const string JobDescriptionRequired = "JobDescription is required.";
    public const string JobRequirementsRequired = "JobRequirements is required.";
    public const string JobRequiredSkillsRequired = "JobRequiredSkills is required.";
    public const string JobRequiredSkillsInvalid = "JobRequiredSkills must be a valid JSON array.";
    public const string JobExperienceYearsRequired = "JobExperienceYears is required.";
    public const string JobExperienceYearsInvalid = "JobExperienceYears must be between 0 and 50.";
    public const string JobEducationTooLong = "JobEducation must be at most 500 characters.";
    public const string JobCreatedByHrIdRequired = "JobCreatedByHrId is required.";

    // Candidate request messages
    public const string CandidateFirstNameRequired = "CandidateFirstName is required.";
    public const string CandidateFirstNameTooLong = "CandidateFirstName must be at most 100 characters.";
    public const string CandidateLastNameRequired = "CandidateLastName is required.";
    public const string CandidateLastNameTooLong = "CandidateLastName must be at most 100 characters.";
    public const string CandidateEmailRequired = "CandidateEmail is required.";
    public const string CandidateEmailInvalid = "CandidateEmail must be a valid email address.";
    public const string CandidateEmailTooLong = "CandidateEmail must be at most 150 characters.";
    public const string CandidatePhoneRequired = "CandidatePhone is required.";
    public const string CandidatePhoneTooLong = "CandidatePhone must be at most 20 characters.";

    // JobApplication request messages
    public const string JobApplicationCandidateIdRequired = "JobApplicationCandidateId is required.";
    public const string JobApplicationJobIdRequired = "JobApplicationJobId is required.";
    public const string JobApplicationCvUrlRequired = "CV file is required. Please upload your CV.";
    public const string JobApplicationCvFileInvalidType = "Invalid file type. Only PDF and DOC/DOCX files are allowed.";
    public const string JobApplicationCvFileTooLarge = "File size must be less than 10MB.";
    public const string JobApplicationScoreInvalid = "Score must be between 0 and 100.";
    public const string JobApplicationStatusInvalid = "Invalid application status.";
}
