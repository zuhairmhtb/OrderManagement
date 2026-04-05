using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Customer;

public class UpdateAddressCommand
{
    [Required]
    public Guid AddressId { get; set; }

    [MaxLength(200)]
    public string? Street { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
}