using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Models;
using OrderManagement.Database.Seeds;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;
    private readonly ApplicationDbContext _context;

    public CustomerService(ILogger<CustomerService> logger, IMapper mapper, ApplicationDbContext context)
    {
        _logger = logger;
        _mapper = mapper;
        _context = context;
    }

	/// <summary>
    /// Adds a new address for an existing customer.
    /// Verifies the customer exists before inserting to avoid a silent FK violation.
    /// </summary>
    public async Task<CustomerProfileDto> AddAddressAsync(AddAddressCommand command)
    {
        // AnyAsync: single EXISTS query – does not load the entity into memory
        bool customerExists = await _context.Customers
            .AnyAsync(c => c.Id == command.CustomerId);

        if (!customerExists)
            throw new KeyNotFoundException($"Customer '{command.CustomerId}' was not found.");

        var address = _mapper.Map<Address>(command);

        await _context.CustomerAddresses.AddAsync(address);
        await _context.SaveChangesAsync();

        return _mapper.Map<CustomerProfileDto>(await _context.Customers
			.AsNoTracking()
			.Include(c => c.Addresses)
			.FirstAsync(c => c.Id == command.CustomerId));
    }

	public async Task<CustomerProfileDto> RemoveAddressAsync(RemoveAddressCommand command)
	{
		// Single query with ownership check – avoids a separate customer lookup
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.CustomerId == command.CustomerId);

        if (address is null)
            throw new KeyNotFoundException(
                $"Address '{command.AddressId}' for customer '{command.CustomerId}' was not found.");

        // Partial update: AutoMapper skips null source values (configured via ForAllMembers in MappingProfile)
        _context.CustomerAddresses.Remove(address);
        await _context.SaveChangesAsync();

		return _mapper.Map<CustomerProfileDto>(await _context.Customers
			.AsNoTracking()
			.Include(c => c.Addresses)
			.FirstAsync(c => c.Id == command.CustomerId));

	}

	public async Task<CustomerProfileDto> UpdateAddressAsync(UpdateAddressCommand command)
	{
		// Single query with ownership check – avoids a separate customer lookup
        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.CustomerId == command.CustomerId);

        if (address is null)
            throw new KeyNotFoundException(
                $"Address '{command.AddressId}' for customer '{command.CustomerId}' was not found.");

        // Partial update: AutoMapper skips null source values (configured via ForAllMembers in MappingProfile)
        _mapper.Map(command, address);

        await _context.SaveChangesAsync();

		return _mapper.Map<CustomerProfileDto>(await _context.Customers
			.AsNoTracking()
			.Include(c => c.Addresses)
			.FirstAsync(c => c.Id == command.CustomerId));
	}

	public async Task<CustomerProfileDto> UpdateProfileAsync(UpdateProfileCommand command)
	{
		// FindAsync uses PK – fastest EF lookup path
        var customer = await _context.Customers.FindAsync(command.CustomerId);

        if (customer is null)
            throw new KeyNotFoundException($"Customer '{command.CustomerId}' was not found.");

        // Partial update: AutoMapper skips null source values (configured via ForAllMembers in MappingProfile)
        _mapper.Map(command, customer);
		 _context.Update(customer);
        await _context.SaveChangesAsync();

        return _mapper.Map<CustomerProfileDto>(await _context.Customers
			.AsNoTracking()
			.FirstAsync(c => c.Id == command.CustomerId)
			);
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

	public async Task<CustomerProfileDto> PopulateSampleDataAsync()
	{
		try
		{
			_logger.LogInformation("Creating and saving a random customer to the database");

			// Generate a single random customer using CustomerSeed
			var randomCustomers = CustomerSeed.GetCustomers(1);
			var newCustomer = randomCustomers.First();

			// Check if a customer with this email already exists
			var existingCustomer = await _context.Customers
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Email == newCustomer.Email);

			if (existingCustomer != null)
			{
				_logger.LogInformation("Customer with email {Email} already exists, returning existing customer", newCustomer.Email);
				return _mapper.Map<CustomerProfileDto>(existingCustomer);
			}

			// Add the new customer to the database
			_context.Customers.Add(newCustomer);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Successfully created customer with ID: {CustomerId} and email: {Email}", 
				newCustomer.Id, newCustomer.Email);

			// Return the created customer as CustomerProfileDto
			return _mapper.Map<CustomerProfileDto>(newCustomer);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while creating and saving random customer");
			throw;
		}
	}

	public async Task<CustomerProfileDto> GetRandomCustomerAsync()
	{
		try
		{
			_logger.LogInformation("Fetching a random customer from the database");

			// Get the total count of customers
			var totalCustomers = await _context.Customers.CountAsync();

			if (totalCustomers == 0)
			{
				_logger.LogWarning("No customers found in the database");
				throw new InvalidOperationException("No customers exist in the database");
			}

			// Generate a random index
			var random = new Random();
			var randomIndex = random.Next(0, totalCustomers);

			// Fetch a random customer using Skip
			var randomCustomer = await _context.Customers
				.AsNoTracking()
				.Include(c => c.Addresses)
				.Skip(randomIndex)
				.FirstAsync();

			_logger.LogInformation("Successfully retrieved random customer with ID: {CustomerId}", randomCustomer.Id);

			// Return the customer as CustomerProfileDto
			return _mapper.Map<CustomerProfileDto>(randomCustomer);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while fetching random customer");
			throw;
		}
	}
}
