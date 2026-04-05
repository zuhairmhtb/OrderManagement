using AutoMapper;
using MassTransit;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Dtos.Order;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class OrderService : IOrderService
{
	private readonly ILogger<OrderService> _logger;
	private readonly IMapper _mapper;
	private readonly IPublishEndpoint _publishEndpoint;

	public OrderService(ILogger<OrderService> logger, IMapper mapper, IPublishEndpoint publishEndpoint)
	{
		_logger = logger;
		_mapper = mapper;
		_publishEndpoint = publishEndpoint;
	}

	public async Task<OrderStatus> PlaceOrderAsync(PlaceOrderCommand command)
	{
		await _publishEndpoint.Publish(command);
		return OrderStatus.Pending;
	}

	public Task<CustomerOrderDto> GetOrderDetailsAsync(Guid orderId)
	{
		throw new NotImplementedException();
	}

	public Task<OrderStatus> GetOrderStatusAsync(Guid orderId)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}
}
