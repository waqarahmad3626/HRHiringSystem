using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRHiringSystem.Domain.Entities;

public class Job : AuditableEntity
{
    [Key]
    [Required(ErrorMessage = "Job Id is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Job Title is required.")]
    [MaxLength(150)]
    public required string Title { get; set; }

    [Required(ErrorMessage = "Job Description is required.")]
    public required string Description { get; set; }

    [Required(ErrorMessage = "Job Requirements are required.")]
    public required string Requirements { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Created By HR Id is required.")]
    public Guid CreatedByHrId { get; set; }

    [ForeignKey("CreatedByHrId")]
    public User? CreatedByHr { get; set; }

    // Navigation property
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}