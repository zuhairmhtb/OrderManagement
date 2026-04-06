using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Customer;

public class RemoveAddressCommand
{
    [Required]
    public Guid AddressId { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
}