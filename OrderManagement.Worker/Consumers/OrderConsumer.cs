using MassTransit;
using Microsoft.Extensions.Logging;
using OrderManagement.Database.Events.Order;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Worker.Consumers;

public class OrderConsumer: IConsumer<PlacedOrderEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderConsumer> _logger;

    public OrderConsumer(IOrderService orderService, ILogger<OrderConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }


    public async Task Consume(ConsumeContext<PlacedOrderEvent> eventContext)
    {
        var command = eventContext.Message;
        _logger.Log(LogLevel.Information, $"Received PlacedOrderEvent for orderId: {command.OrderId}");
        try
        {
            var order = await _orderService.PlaceOrderAsync(command);
            _logger.Log(LogLevel.Information, $"Successfully processed PlaceOrderCommand for orderId: {command.OrderId}, OrderId: {order.OrderId}");
        } catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing PlaceOrderCommand for orderId: {command.OrderId}");
            // Optionally, you could rethrow or handle the exception based on your needs
        }
    }
}