using System.ComponentModel.DataAnnotations;
using OrderManagement.Database.Dtos;

namespace OrderManagement.Database.Commands;

public class PlaceOrderDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one product is required")]
    public List<OrderItemDto> Products { get; set; } = new();

    [Required]
    public int CustomerId { get; set; }

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

public class OrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}
