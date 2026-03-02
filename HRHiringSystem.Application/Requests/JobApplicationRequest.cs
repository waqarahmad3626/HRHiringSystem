using System;
using Microsoft.AspNetCore.Http;

namespace HRHiringSystem.Application.Requests;

public class JobApplicationRequest
{
    // Candidate information supplied at application time
    public required string CandidateFirstName { get; set; }
    public required string CandidateLastName { get; set; }
    public required string CandidateEmail { get; set; }
    public required string CandidatePhone { get; set; }

    // Job being applied to
    public Guid JobId { get; set; }

    // CV file uploaded by candidate (PDF, DOCX, etc.)
    public IFormFile? CvFile { get; set; }
}
