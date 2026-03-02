namespace HRHiringSystem.Application.Constants;

public static class ResponseConstants
{
    // Success messages
    public const string UserCreated = "User created successfully.";
    public const string UserRetrieved = "User retrieved successfully.";
    public const string UsersRetrieved = "Users retrieved successfully.";
    public const string UserUpdated = "User updated successfully.";
    public const string UserDeleted = "User deleted successfully.";

    public const string RoleCreated = "Role created successfully.";
    public const string RoleRetrieved = "Role retrieved successfully.";
    public const string RolesRetrieved = "Roles retrieved successfully.";
    public const string RoleUpdated = "Role updated successfully.";
    public const string RoleDeleted = "Role deleted successfully.";

    public const string JobCreated = "Job created successfully.";
    public const string JobRetrieved = "Job retrieved successfully.";
    public const string JobsRetrieved = "Jobs retrieved successfully.";
    public const string JobUpdated = "Job updated successfully.";
    public const string JobDeleted = "Job deleted successfully.";

    public const string CandidateCreated = "Candidate created successfully.";
    public const string CandidateRetrieved = "Candidate retrieved successfully.";
    public const string CandidatesRetrieved = "Candidates retrieved successfully.";
    public const string CandidateUpdated = "Candidate updated successfully.";
    public const string CandidateDeleted = "Candidate deleted successfully.";

    public const string JobApplicationCreated = "Job application created successfully.";
    public const string JobApplicationRetrieved = "Job application retrieved successfully.";
    public const string JobApplicationsRetrieved = "Job applications retrieved successfully.";
    public const string JobApplicationUpdated = "Job application updated successfully.";
    public const string JobApplicationDeleted = "Job application deleted successfully.";
    public const string AlreadyApplied = "You have already applied to this job.";

    // Error codes
    public const string ErrorCodeNotFound = "NotFound";
    public const string ErrorCodeValidation = "Validation";
    public const string ErrorCodeException = "Exception";

    // Generic messages
    public const string GenericException = "An unexpected error occurred.";
    
    // Not found messages
    public const string UserNotFound = "User not found";
    public const string RoleNotFound = "Role not found";
    public const string JobNotFound = "Job not found";
    public const string CandidateNotFound = "Candidate not found";
    public const string JobApplicationNotFound = "JobApplication not found";
    public const string EmailAlreadyExists = "Email already in use.";
    public const string RoleNameAlreadyExists = "Role with the same name already exists.";
}
