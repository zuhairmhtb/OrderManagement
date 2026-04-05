using AutoMapper;
using MassTransit;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CustomerService(ILogger<CustomerService> logger, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

	public async Task<bool> AddAddressAsync(AddAddressCommand command)
	{
		await _publishEndpoint.Publish(command);
		return true;
	}

	public async Task<bool> RemoveAddressAsync(RemoveAddressCommand command)
	{
		await _publishEndpoint.Publish(command);
		return true;
	}

	public async Task<bool> UpdateAddressAsync(UpdateAddressCommand command)
	{
		await _publishEndpoint.Publish(command);
		return true;
	}

	public async Task<bool> UpdateProfileAsync(UpdateProfileCommand command)
	{
		await _publishEndpoint.Publish(command);
		return true;
	}

    public async Task<CustomerProfileDto> GetCustomerProfileAsync(Guid customerId)
	{
		throw new NotImplementedException();
        
	}

	public async Task<IEnumerable<CustomerProfileDto>> SearchCustomersAsync(
		string? emailPattern = null,
		string? namePattern = null,
		int page = 1,
		int pageSize = 20)
	{
		throw new NotImplementedException();
	}
}
