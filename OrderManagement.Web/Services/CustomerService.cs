using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ApplicationDbContext _context;

    public CustomerService(ILogger<CustomerService> logger, IMapper mapper, IPublishEndpoint publishEndpoint, ApplicationDbContext context)
    {
        _logger = logger;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _context = context;
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
		try
		{
			_logger.LogInformation("Getting customer profile for customer ID: {CustomerId}", customerId);

			var customer = await _context.Customers
				.AsNoTracking()
				.Include(c => c.Addresses) // Include addresses if needed in the profile
				.FirstOrDefaultAsync(c => c.Id == customerId);

			if (customer == null)
			{
				_logger.LogWarning("Customer not found for ID: {CustomerId}", customerId);
				throw new ArgumentException($"Customer with ID {customerId} not found.");
			}

			var customerProfileDto = _mapper.Map<CustomerProfileDto>(customer);
			_logger.LogInformation("Successfully retrieved customer profile for ID: {CustomerId}", customerId);

			return customerProfileDto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while getting customer profile for ID: {CustomerId}", customerId);
			throw;
		}
	}

	public async Task<IEnumerable<CustomerProfileDto>> SearchCustomersAsync(
		string? emailPattern = null,
		string? namePattern = null,
		int page = 1,
		int pageSize = 20)
	{
		try
		{
			_logger.LogInformation("Searching customers with emailPattern: {EmailPattern}, namePattern: {NamePattern}, page: {Page}, pageSize: {PageSize}", 
				emailPattern, namePattern, page, pageSize);

			// Validate pagination parameters
			if (page < 1) page = 1;
			if (pageSize < 1 || pageSize > 100) pageSize = 20; // Limit max page size for performance

			var query = _context.Customers.AsNoTracking().AsQueryable();

			// Apply email filter if provided
			if (!string.IsNullOrWhiteSpace(emailPattern))
			{
				query = query.Where(c => EF.Functions.Like(c.Email, $"%{emailPattern}%"));
			}

			// Apply name filter if provided - search in both FirstName and LastName
			if (!string.IsNullOrWhiteSpace(namePattern))
			{
				query = query.Where(c => 
					(c.FirstName != null && EF.Functions.Like(c.FirstName, $"%{namePattern}%")) ||
					(c.LastName != null && EF.Functions.Like(c.LastName, $"%{namePattern}%"))
				);
			}

			// Apply pagination and order by email for consistent results
			var customers = await query
				.OrderBy(c => c.Email)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			var customerProfileDtos = _mapper.Map<IEnumerable<CustomerProfileDto>>(customers);

			_logger.LogInformation("Successfully retrieved {Count} customers from search", customers.Count);

			return customerProfileDtos;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while searching customers with emailPattern: {EmailPattern}, namePattern: {NamePattern}", 
				emailPattern, namePattern);
			throw;
		}
	}
}
