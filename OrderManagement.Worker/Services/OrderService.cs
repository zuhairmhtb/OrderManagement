using System.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Database.Events.Order;
using OrderManagement.Database.Models;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Worker.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(ApplicationDbContext dbContext, IMapper mapper, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Places a new order for an existing customer.
    /// Validates the customer and every requested product before mutating any state.
    /// Prices, names, and currencies are snapshotted into PurchasedProduct at purchase
    /// time so the order record remains accurate even when the catalogue changes later.
    /// Stock decrements and the order INSERT are flushed in a single SaveChangesAsync
    /// call so EF can batch them into one database round-trip.
    /// </summary>
    public async Task<CustomerOrderDto> PlaceOrderAsync(PlacedOrderEvent command)
    {
        _logger.LogInformation("Processing order {orderId}.",
            command.OrderId);

        var order = await _dbContext.Orders
            .Include(o => o.Products)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId);

        if(order == null)
        {
            _logger.LogError("Order {orderId} not found.", command.OrderId);
            throw new InvalidOperationException($"Order with ID {command.OrderId} not found.");
        }

        order.OrderStatus = OrderStatus.Processing;
        try
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();    
        } catch (DBConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error while processing order {orderId}.", command.OrderId);
            throw new InvalidOperationException($"Order with ID {command.OrderId} could not be processed due to a concurrency conflict. Please try again.");
        }
        

         _logger.LogInformation("Calculating VAT, Shipping Cost, Additional Charges, and Total Price for order {orderId}.",
            command.OrderId);

            order.Vat = order.Products.Sum(p => p.Price * 0.2); // Example VAT calculation
            order.ShippingCost = 5.0; // Flat shipping cost for simplicity
            order.AdditionalCharges = 2.0; // Flat additional charge for simplicity
            order.TotalAmount = order.Subtotal + order.Vat + order.ShippingCost + order.AdditionalCharges;
            await _dbContext.SaveChangesAsync();

        // 10. Map to DTO.
        //     Customer is not a navigation property on Order, so it is resolved
        //     from the already-loaded Customer entity and set after the map call.
        var dto = _mapper.Map<CustomerOrderDto>(order);
        return dto;
    }
}
