using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;
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
    public async Task<CustomerOrderDto> PlaceOrderAsync(PlaceOrderCommand command)
    {
        // 1. Validate customer — FindAsync checks the EF identity map before hitting the DB
        var customer = await _dbContext.Customers.FindAsync(command.CustomerId);
        if (customer is null)
            throw new KeyNotFoundException($"Customer '{command.CustomerId}' was not found.");

        // 2. A contact number is required on every order; validate before doing further work
        if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            throw new InvalidOperationException(
                $"Customer '{command.CustomerId}' has no phone number on record. " +
                "A contact number is required to place an order.");

        // 3. Parse the currency enum early so an invalid value fails fast
        if (!Enum.TryParse<Currency>(command.Currency, ignoreCase: true, out var currency))
            throw new ArgumentException($"Invalid currency code '{command.Currency}'.");

        // 4. Load all requested products in a single batch query
        var requestedIds = command.Products.Select(p => p.ProductId).Distinct().ToList();
        var products = await _dbContext.Products
            .Where(p => requestedIds.Contains(p.Id))
            .ToListAsync();

        // 5. Verify every requested product exists
        var missingIds = requestedIds.Except(products.Select(p => p.Id)).ToList();
        if (missingIds.Count > 0)
            throw new KeyNotFoundException(
                $"The following products were not found: {string.Join(", ", missingIds)}.");

        var productLookup = products.ToDictionary(p => p.Id);

        // 6. Validate stock for ALL items before mutating anything — avoids partial state
        foreach (var item in command.Products)
        {
            var product = productLookup[item.ProductId];
            if (product.Quantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. " +
                    $"Available: {product.Quantity}, requested: {item.Quantity}.");
        }

        // 7. Snapshot prices into PurchasedProduct and decrement stock.
        //    EF change tracking records the stock decrements as pending UPDATEs.
        var purchasedProducts = new List<PurchasedProduct>();
        double subtotal = 0;

        foreach (var item in command.Products)
        {
            var product = productLookup[item.ProductId];

            purchasedProducts.Add(new PurchasedProduct
            {
                ProductId = product.Id,
                Name      = product.Name,
                Price     = product.Price,
                Currency  = product.Currency,
                Quantity  = item.Quantity
            });

            subtotal         += product.Price * item.Quantity;
            product.Quantity -= item.Quantity;
        }

        // 8. Build the Order entity via AutoMapper.
        //    Address flattening, CustomerId, and OrderStatus=Pending are handled by the map.
        //    Fields that require the loaded Customer, parsed enum, or computed totals
        //    are set explicitly here since they are Ignored in the mapping profile.
        var order = _mapper.Map<Order>(command);
        order.Currency              = currency;
        order.CustomerEmail         = customer.Email;
        order.CustomerContactNumber = customer.PhoneNumber!;
        order.Subtotal              = subtotal;
        order.TotalAmount           = subtotal;
        order.Products              = purchasedProducts;
        order.OrderStatus           = OrderStatus.Pending; 

        // 9. Single SaveChangesAsync — EF batches the Order INSERT, PurchasedProduct
        //    INSERTs, and all stock UPDATEs into one database round-trip
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} placed for customer {CustomerId} — {ItemCount} item(s), total {Total} {Currency}.",
            order.Id, customer.Id, purchasedProducts.Count, order.TotalAmount, order.Currency);

        // 10. Map to DTO.
        //     Customer is not a navigation property on Order, so it is resolved
        //     from the already-loaded Customer entity and set after the map call.
        var dto = _mapper.Map<CustomerOrderDto>(order);
        dto.Customer = _mapper.Map<CustomerProfileDto>(customer);

        return dto;
    }
}
