using System.ComponentModel.DataAnnotations;

namespace HRHiringSystem.Domain.Entities;

public class Candidate : AuditableEntity
{
    [Key]
    [Required(ErrorMessage = "Candidate Id is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "First Name is required.")]
    [MaxLength(100)]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    [MaxLength(100)]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [MaxLength(150)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Phone is required.")]
    [Phone]
    [MaxLength(20)]
    public required string Phone { get; set; }
}