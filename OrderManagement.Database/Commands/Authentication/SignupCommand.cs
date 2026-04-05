using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Authentication;

public class SignupCommand
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string ConfirmPassword { get; set; } = null!;

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}