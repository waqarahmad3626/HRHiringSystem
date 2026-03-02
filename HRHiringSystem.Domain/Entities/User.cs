using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRHiringSystem.Domain.Entities;

public class User : AuditableEntity
{
    [Key]
    [Required(ErrorMessage = "User Id is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "User Name is required.")]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [MaxLength(150)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public required string PasswordHash { get; set; }

    [Required(ErrorMessage = "Role Id is required.")]
    public Guid RoleId { get; set; }

    [ForeignKey("RoleId")]
    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
}