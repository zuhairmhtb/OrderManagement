using OrderManagement.Database.Dtos.Customer;

namespace OrderManagement.Database.Dtos.Order;

public class CustomerOrderDto {
    public Guid OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public double Subtotal { get; set; }
    public double Total { get; set; }
    public double Vat { get; set; }
    public double ShippingCost { get; set; }
    public double AdditionalCharges { get; set; }
    public string Currency { get; set; } = null!;
    public string OrderStatus { get; set; } = null!;

    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto? BillingAddress { get; set; }
    public CustomerProfileDto Customer { get; set; } = null!;

    public IEnumerable<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    public string CustomerEmail { get; set; } = null!;

}