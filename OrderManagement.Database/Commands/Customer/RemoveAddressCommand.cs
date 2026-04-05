using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Database.Commands.Customer;

public class RemoveAddressCommand
{
    [Required]
    public Guid AddressId { get; set; }
}