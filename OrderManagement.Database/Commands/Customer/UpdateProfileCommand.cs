using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Customer;

public class UpdateProfileCommand
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
}
