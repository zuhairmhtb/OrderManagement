using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Customer;

public class AddAddressCommand
{
    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = null!;

    [MaxLength(100)]
    public string? State { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
}