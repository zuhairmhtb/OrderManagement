using MassTransit;
using Microsoft.Extensions.Logging;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Worker.Consumers;

public class CustomerConsumer :
    IConsumer<UpdateProfileCommand>,
    IConsumer<AddAddressCommand>,
    IConsumer<UpdateAddressCommand>,
    IConsumer<RemoveAddressCommand>
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerConsumer> _logger;

    public CustomerConsumer(ICustomerService customerService, ILogger<CustomerConsumer> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UpdateProfileCommand> context)
    {
        var command = context.Message;
        _logger.Log(LogLevel.Information, $"Received UpdateProfileCommand for CustomerId: {command.CustomerId}");
        try
        {
            var profile = await _customerService.UpdateProfileAsync(command);
            _logger.Log(LogLevel.Information, $"Successfully processed UpdateProfileCommand for CustomerId: {command.CustomerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing UpdateProfileCommand for CustomerId: {command.CustomerId}");
        }
    }

    public async Task Consume(ConsumeContext<AddAddressCommand> context)
    {
        var command = context.Message;
        _logger.Log(LogLevel.Information, $"Received AddAddressCommand for CustomerId: {command.CustomerId}");
        try
        {
            var address = await _customerService.AddAddressAsync(command);
            _logger.Log(LogLevel.Information, $"Successfully processed AddAddressCommand for CustomerId: {command.CustomerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing AddAddressCommand for CustomerId: {command.CustomerId}");
        }
    }

    public async Task Consume(ConsumeContext<UpdateAddressCommand> context)
    {
        var command = context.Message;
        _logger.Log(LogLevel.Information, $"Received UpdateAddressCommand for AddressId: {command.AddressId}, CustomerId: {command.CustomerId}");
        try
        {
            var address = await _customerService.UpdateAddressAsync(command);
            _logger.Log(LogLevel.Information, $"Successfully processed UpdateAddressCommand for AddressId: {command.AddressId}, CustomerId: {command.CustomerId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing UpdateAddressCommand for AddressId: {command.AddressId}, CustomerId: {command.CustomerId}");
        }
    }

    public async Task Consume(ConsumeContext<RemoveAddressCommand> context)
    {
        var command = context.Message;
        _logger.Log(LogLevel.Information, $"Received RemoveAddressCommand for AddressId: {command.AddressId}");
        try
        {
            var removed = await _customerService.RemoveAddressAsync(command);
            if (removed)
                _logger.Log(LogLevel.Information, $"Successfully processed RemoveAddressCommand for AddressId: {command.AddressId}");
            else
                _logger.Log(LogLevel.Warning, $"RemoveAddressCommand for AddressId: {command.AddressId} had no effect — address was not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing RemoveAddressCommand for AddressId: {command.AddressId}");
        }
    }
}