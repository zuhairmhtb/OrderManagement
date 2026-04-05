using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Dtos.Order;

namespace OrderManagement.Web.Interfaces;

public interface IOrderService
{
    Task<OrderStatus> PlaceOrderAsync(PlaceOrderCommand command);
    Task<CustomerOrderDto> GetOrderDetailsAsync(Guid orderId);
    Task<OrderStatus> GetOrderStatusAsync(Guid orderId);
    Task<CustomerOrderDto[]> SearchOrdersAsync(
        DateTime? placedAtStartRange = null,
        DateTime? placedAtEndRange = null,
        DateTime? deliveredOnStartRange = null,
        DateTime? deliveredOnEndRange = null,
        Guid? customerId = null,
        string? customerEmailPattern = null,
        OrderStatus? status = null,
        int page = 1,
        int pageSize = 20
    );
}