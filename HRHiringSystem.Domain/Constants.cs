
namespace HRHiringSystem.Domain.Constants;

public static class ValidationMessages
{
    public static class User
    {
        public const string NameRequired = "User name is required.";
        public const string EmailRequired = "User email is required.";
        public const string EmailInvalid = "User email format is invalid.";
        public const string PasswordRequired = "Password is required.";
        public const string RoleRequired = "Role is required.";
    }

    public static class Role
    {
        public const string NameRequired = "Role name is required.";
        public const string DescriptionRequired = "Role description is required.";
    }

    public static class Candidate
    {
        public const string FirstNameRequired = "Candidate first name is required.";
        public const string LastNameRequired = "Candidate last name is required.";
        public const string EmailRequired = "Candidate email is required.";
        public const string EmailInvalid = "Candidate email format is invalid.";
        public const string PhoneRequired = "Candidate phone number is required.";
    }

    public static class Job
    {
        public const string TitleRequired = "Job title is required.";
        public const string DescriptionRequired = "Job description is required.";
        public const string RequirementsRequired = "Job requirements are required.";
        public const string RequiredSkillsRequired = "Required skills are required.";
        public const string ExperienceYearsRequired = "Experience years is required.";
        public const string ExperienceYearsInvalid = "Experience years must be between 0 and 50.";
        public const string CreatedByRequired = "Created By HR is required.";
    }

    public static class JobApplication
    {
        public const string CandidateRequired = "Candidate Id is required.";
        public const string JobRequired = "Job Id is required.";
        public const string CvRequired = "CV URL is required.";
        public const string ScoreInvalid = "Score must be between 0 and 100.";
        public const string StatusInvalid = "Invalid application status.";
    }

}
