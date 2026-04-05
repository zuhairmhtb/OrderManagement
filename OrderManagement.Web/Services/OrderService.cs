using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Order;
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
}
