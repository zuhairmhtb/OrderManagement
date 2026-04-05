using AutoMapper;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Context;
using OrderManagement.Database.Dtos.Customer;
using OrderManagement.Database.Models;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Worker.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger _logger ;

    public CustomerService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Adds a new address for an existing customer.
    /// Verifies the customer exists before inserting to avoid a silent FK violation.
    /// </summary>
    public async Task<AddressDto> AddAddressAsync(AddAddressCommand command)
    {
        // AnyAsync: single EXISTS query – does not load the entity into memory
        bool customerExists = await _dbContext.Customers
            .AnyAsync(c => c.Id == command.CustomerId);

        if (!customerExists)
            throw new KeyNotFoundException($"Customer '{command.CustomerId}' was not found.");

        var address = _mapper.Map<Address>(command);

        await _dbContext.CustomerAddresses.AddAsync(address);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    /// <summary>
    /// Applies a partial update to an existing address.
    /// Only non-null fields in the command overwrite the stored values.
    /// Ownership is verified by requiring both AddressId and CustomerId to match.
    /// </summary>
    public async Task<AddressDto> UpdateAddressAsync(UpdateAddressCommand command)
    {
        // Single query with ownership check – avoids a separate customer lookup
        var address = await _dbContext.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == command.AddressId && a.CustomerId == command.CustomerId);

        if (address is null)
            throw new KeyNotFoundException(
                $"Address '{command.AddressId}' for customer '{command.CustomerId}' was not found.");

        // Partial update: AutoMapper skips null source values (configured via ForAllMembers in MappingProfile)
        _mapper.Map(command, address);

        await _dbContext.SaveChangesAsync();

        return _mapper.Map<AddressDto>(address);
    }

    /// <summary>
    /// Removes an address by its primary key.
    /// Returns false (instead of throwing) when the address does not exist,
    /// making delete idempotent for callers that do not care about prior existence.
    /// </summary>
    public async Task<bool> RemoveAddressAsync(RemoveAddressCommand command)
    {
        // FindAsync hits the EF identity map first, then the DB by PK – optimal for PK lookups
        var address = await _dbContext.CustomerAddresses.FindAsync(command.AddressId);

        if (address is null)
            return false;

        _dbContext.CustomerAddresses.Remove(address);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Applies a partial update to a customer's profile.
    /// Only non-null fields in the command overwrite the stored values.
    /// </summary>
    public async Task<CustomerProfileDto> UpdateProfileAsync(UpdateProfileCommand command)
    {
        // FindAsync uses PK – fastest EF lookup path
        var customer = await _dbContext.Customers.FindAsync(command.CustomerId);

        if (customer is null)
            throw new KeyNotFoundException($"Customer '{command.CustomerId}' was not found.");

        // Partial update: AutoMapper skips null source values (configured via ForAllMembers in MappingProfile)
        _mapper.Map(command, customer);

        await _dbContext.SaveChangesAsync();

        return _mapper.Map<CustomerProfileDto>(customer);
    }

}