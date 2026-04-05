using System.ComponentModel.DataAnnotations;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;

namespace OrderManagement.Database.Commands.Order;

public class PlaceOrderCommand
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one product is required")]
    public List<OrderItemDto> Products { get; set; } = new();

    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public AddressDto ShippingAddress { get; set; } = null!;

    [Required]
    public AddressDto BillingAddress { get; set; } = null!;

    /// <summary>
    /// ISO 4217 currency code (e.g. USD, EUR, GBP).
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = null!;
}


