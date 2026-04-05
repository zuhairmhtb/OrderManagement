using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Authentication;

public class UpdatePasswordCommand
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string ConfirmPassword { get; set; } = null!;
}