using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Dtos.Order;

namespace OrderManagement.Web.Interfaces;

public interface IOrderService
{
    Task<CustomerOrderDto> PlaceOrderAsync(PlaceOrderCommand command);
}