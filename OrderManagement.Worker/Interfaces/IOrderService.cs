using OrderManagement.Database.Dtos.Order;
using OrderManagement.Database.Events.Order;

namespace OrderManagement.Web.Interfaces;

public interface IOrderService
{
    Task<CustomerOrderDto> PlaceOrderAsync(PlacedOrderEvent command);
}