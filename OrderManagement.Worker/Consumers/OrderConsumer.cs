using MassTransit;
using Microsoft.Extensions.Logging;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Worker.Consumers;

public class OrderConsumer: IConsumer<PlaceOrderCommand>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderConsumer> _logger;

    public OrderConsumer(IOrderService orderService, ILogger<OrderConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }


    public async Task Consume(ConsumeContext<PlaceOrderCommand> context)
    {
        var command = context.Message;
        _logger.Log(LogLevel.Information, $"Received PlaceOrderCommand for CustomerId: {command.CustomerId}");
        try
        {
            var order = await _orderService.PlaceOrderAsync(command);
            _logger.Log(LogLevel.Information, $"Successfully processed PlaceOrderCommand for CustomerId: {command.CustomerId}, OrderId: {order.OrderId}");
        } catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing PlaceOrderCommand for CustomerId: {command.CustomerId}");
            // Optionally, you could rethrow or handle the exception based on your needs
        }
    }
}