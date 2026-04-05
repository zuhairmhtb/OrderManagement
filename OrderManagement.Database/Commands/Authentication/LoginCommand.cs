using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Authentication;

public class LoginCommand
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = null!;
}