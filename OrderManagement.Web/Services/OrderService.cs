using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Database.Models;
using OrderManagement.Database.Seeds;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class OrderService : IOrderService
{
	private readonly ILogger<OrderService> _logger;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;
	private readonly ApplicationDbContext _context;

	public OrderService(ILogger<OrderService> logger, IMapper mapper, IPublishEndpoint publishEndpoint, ApplicationDbContext context)
	{
		_logger = logger;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
		_context = context;
	}

	public async Task<OrderStatus> PlaceOrderAsync(PlaceOrderCommand command)
	{
		await _publishEndpoint.Publish(command);
		return OrderStatus.Pending;
	}

	public async Task<CustomerOrderDto> GetOrderDetailsAsync(Guid orderId)
	{
		try
		{
			_logger.LogInformation("Getting order details for order ID: {OrderId}", orderId);

			var order = await _context.Orders
				.AsNoTracking()
				.Include(o => o.Products)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
			{
				_logger.LogWarning("Order not found for ID: {OrderId}", orderId);
				throw new ArgumentException($"Order with ID {orderId} not found.");
			}

			// Get customer details
			var customer = await _context.Customers
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == order.CustomerId);

			var orderDto = _mapper.Map<CustomerOrderDto>(order);
			
			// Map customer information if available
			if (customer != null)
			{
				orderDto.Customer = _mapper.Map<Database.Dtos.Customer.CustomerProfileDto>(customer);
			}

			_logger.LogInformation("Successfully retrieved order details for ID: {OrderId}", orderId);
			return orderDto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting order details for ID: {OrderId}", orderId);
			throw;
		}
	}

	public async Task<OrderStatus> GetOrderStatusAsync(Guid orderId)
	{
		try
		{
			_logger.LogInformation("Getting order status for order ID: {OrderId}", orderId);

			var orderStatus = await _context.Orders
				.AsNoTracking()
				.Where(o => o.Id == orderId)
				.Select(o => o.OrderStatus)
				.FirstOrDefaultAsync();

			if (orderStatus == default(OrderStatus))
			{
				_logger.LogWarning("Order not found for status check, ID: {OrderId}", orderId);
				throw new ArgumentException($"Order with ID {orderId} not found.");
			}

			_logger.LogInformation("Successfully retrieved order status {Status} for ID: {OrderId}", orderStatus, orderId);
			return orderStatus;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting order status for ID: {OrderId}", orderId);
			throw;
		}
	}

	public async Task<IEnumerable<CustomerOrderDto>> SearchOrdersAsync(
		DateTime? placedAtStartRange = null,
		DateTime? placedAtEndRange = null,
		DateTime? deliveredOnStartRange = null,
		DateTime? deliveredOnEndRange = null,
		Guid? customerId = null,
		string? customerEmailPattern = null,
		OrderStatus? status = null,
		int page = 1,
		int pageSize = 20)
	{
		try
		{
			_logger.LogInformation("Searching orders with customerId: {CustomerId}, customerEmailPattern: {EmailPattern}, status: {Status}, page: {Page}, pageSize: {PageSize}", 
				customerId, customerEmailPattern, status, page, pageSize);

			// Validate pagination parameters
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 100) pageSize = 20; // Limit max page size for performance

			var query = _context.Orders
				.AsNoTracking()
				.Include(o => o.Products)
				.AsQueryable();

			// Apply order date range filter
			if (placedAtStartRange.HasValue)
			{
				query = query.Where(o => o.OrderDate >= placedAtStartRange.Value);
			}
			if (placedAtEndRange.HasValue)
			{
				query = query.Where(o => o.OrderDate <= placedAtEndRange.Value);
			}

			// Apply delivery date range filter
			if (deliveredOnStartRange.HasValue)
			{
				query = query.Where(o => o.DeliveryDate.HasValue && o.DeliveryDate >= deliveredOnStartRange.Value);
			}
			if (deliveredOnEndRange.HasValue)
			{
				query = query.Where(o => o.DeliveryDate.HasValue && o.DeliveryDate <= deliveredOnEndRange.Value);
			}

			// Apply customer ID filter
			if (customerId.HasValue)
			{
				query = query.Where(o => o.CustomerId == customerId.Value);
			}

			// Apply customer email filter
			if (!string.IsNullOrWhiteSpace(customerEmailPattern))
			{
				query = query.Where(o => EF.Functions.Like(o.CustomerEmail, $"%{customerEmailPattern}%"));
			}

			// Apply order status filter
			if (status.HasValue)
			{
				query = query.Where(o => o.OrderStatus == status.Value);
			}

			// Apply pagination and ordering
			var orders = await query
				.OrderByDescending(o => o.OrderDate) // Most recent orders first
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Get customer information for all orders
			var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
			var customers = await _context.Customers
				.AsNoTracking()
				.Where(c => customerIds.Contains(c.Id))
				.ToListAsync();

			// Map orders to DTOs
			var orderDtos = new List<CustomerOrderDto>();
			foreach (var order in orders)
			{
				var orderDto = _mapper.Map<CustomerOrderDto>(order);
				
				// Map customer information
				var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
				if (customer != null)
				{
					orderDto.Customer = _mapper.Map<Database.Dtos.Customer.CustomerProfileDto>(customer);
				}
				
				orderDtos.Add(orderDto);
			}

			_logger.LogInformation("Successfully retrieved {Count} orders from search", orderDtos.Count);
			return orderDtos;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while searching orders with customerId: {CustomerId}, customerEmailPattern: {EmailPattern}", 
				customerId, customerEmailPattern);
			throw;
		}
	}

	public async Task<CustomerOrderDto> PopulateSampleDataAsync(CustomerProfileDto? customer)
	{
		try
		{
			_logger.LogInformation("Creating and saving a sample order to the database");

			// If no customer provided, get a random existing customer
			if (customer == null)
			{
				var customerCount = await _context.Customers.CountAsync();
				if (customerCount == 0)
				{
					throw new InvalidOperationException("No customers exist in the database. Please create a customer first.");
				}

				var random = new Random();
				var randomIndex = random.Next(0, customerCount);
				var randomCustomer = await _context.Customers
					.AsNoTracking()
					.Skip(randomIndex)
					.FirstAsync();

				customer = _mapper.Map<CustomerProfileDto>(randomCustomer);
			}

			// Generate a single random order using OrderSeed
			var randomOrders = OrderSeed.GetOrders(1);
			var newOrder = randomOrders.First();

			// Update the order with the provided/selected customer information
			newOrder.CustomerId = customer.Id;
			newOrder.CustomerEmail = customer.Email;
			newOrder.CustomerContactNumber = customer.PhoneNumber ?? "+1-555-000-0000";

			// Get some existing products to add to the order
			var existingProducts = await _context.Products
				.AsNoTracking()
				.Take(3) // Take up to 3 products
				.ToListAsync();

			// If no products exist, create some sample products
			if (!existingProducts.Any())
			{
				var sampleProducts = ProductSeed.GetProducts(3);
				_context.Products.AddRange(sampleProducts);
				await _context.SaveChangesAsync();
				existingProducts = sampleProducts;
			}

			// Create purchased products for the order
			var random2 = new Random();
			var purchasedProducts = new List<PurchasedProduct>();
			double calculatedSubtotal = 0;

			foreach (var product in existingProducts)
			{
				var quantity = random2.Next(1, 4); // 1-3 items per product
				var lineTotal = product.Price * quantity;
				calculatedSubtotal += lineTotal;

				purchasedProducts.Add(new PurchasedProduct
				{
					Id = Guid.NewGuid(),
					ProductId = product.Id,
					Name = product.Name,
					Price = product.Price,
					Currency = product.Currency,
					Quantity = quantity,
					OrderId = newOrder.Id
				});
			}

			// Update order totals based on actual products
			newOrder.Subtotal = calculatedSubtotal;
			newOrder.TotalAmount = calculatedSubtotal + newOrder.Vat + newOrder.ShippingCost + newOrder.AdditionalCharges;
			newOrder.Products = purchasedProducts;

			// Add the order and its products to the database
			_context.Orders.Add(newOrder);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Successfully created order with ID: {OrderId} for customer: {CustomerEmail}", 
				newOrder.Id, newOrder.CustomerEmail);

			// Return the created order as CustomerOrderDto
			var orderDto = _mapper.Map<CustomerOrderDto>(newOrder);
			orderDto.Customer = customer;

			return orderDto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating and saving sample order");
			throw;
		}
	}

	public async Task<CustomerOrderDto> GetRandomOrderAsync()
	{
		try
		{
			_logger.LogInformation("Fetching a random order from the database");

			// Get the total count of orders
			var totalOrders = await _context.Orders.CountAsync();

			if (totalOrders == 0)
			{
				_logger.LogWarning("No orders found in the database");
				throw new InvalidOperationException("No orders exist in the database");
			}

			// Generate a random index
			var random = new Random();
			var randomIndex = random.Next(0, totalOrders);

			// Fetch a random order using Skip
			var randomOrder = await _context.Orders
				.AsNoTracking()
				.Include(o => o.Products)
				.Skip(randomIndex)
				.FirstAsync();

			// Get customer details
			var customer = await _context.Customers
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Id == randomOrder.CustomerId);

			_logger.LogInformation("Successfully retrieved random order with ID: {OrderId}", randomOrder.Id);

			// Map to CustomerOrderDto
			var orderDto = _mapper.Map<CustomerOrderDto>(randomOrder);
			
			// Map customer information if available
			if (customer != null)
			{
				orderDto.Customer = _mapper.Map<CustomerProfileDto>(customer);
			}

			return orderDto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching random order");
			throw;
		}
	}
}
