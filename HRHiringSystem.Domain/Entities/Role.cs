using System.ComponentModel.DataAnnotations;

namespace HRHiringSystem.Domain.Entities;

public class Role : AuditableEntity
{
    [Required(ErrorMessage = "Role Id is required.")]
    public Guid Id { get; set; }
    [Required(ErrorMessage = "Role Name is required.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Role Description is required.")]
    public required string Description { get; set; }
}
