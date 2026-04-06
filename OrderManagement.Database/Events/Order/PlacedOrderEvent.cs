namespace OrderManagement.Database.Events.Order;

public class PlacedOrderEvent
{
    public Guid OrderId { get; set; }
    public DateTime PlacedAt { get; set; }
}