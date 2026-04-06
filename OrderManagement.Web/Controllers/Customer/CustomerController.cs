using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Customer;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Add a new address for customer
    /// </summary>
    /// <param name="command">Add address command</param>
    /// <returns>Success status</returns>
    [HttpPost("address")]
    [Authorize]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressCommand command)
    {
        _logger.LogInformation("Received request to add address for customer ID: {CustomerId}", command.CustomerId);
        try
        {
            var result = await _customerService.AddAddressAsync(command);
            if(result == null)
            {
                _logger.LogWarning("Failed to add address for customer ID: {CustomerId}", command.CustomerId);
                return BadRequest(new { Error = "Failed to add address." });
            }
            return Ok(new { Customer = result, Message = "Address added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding address");
            return StatusCode(500, new { Error = "Failed to add address" });
        }
    }

    /// <summary>
    /// Update an existing address
    /// </summary>
    /// <param name="command">Update address command</param>
    /// <returns>Success status</returns>
    [HttpPut("address")]
    [Authorize]
    public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressCommand command)
    {
        _logger.LogInformation("Received request to update address ID: {AddressId} for customer ID: {CustomerId}", command.AddressId, command.CustomerId);
        try
        {
            var result = await _customerService.UpdateAddressAsync(command);
            if(result == null)
            {
                _logger.LogWarning("Failed to update address ID: {AddressId} for customer ID: {CustomerId}", command.AddressId, command.CustomerId);
                return BadRequest(new { Error = "Failed to update address." });
            }
            return Ok(new { Success = result, Message = "Address updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address");
            return StatusCode(500, new { Error = "Failed to update address" });
        }
    }

    /// <summary>
    /// Remove an address
    /// </summary>
    /// <param name="command">Remove address command</param>
    /// <returns>Success status</returns>
    [HttpDelete("address")]
    [Authorize]
    public async Task<IActionResult> RemoveAddress([FromBody] RemoveAddressCommand command)
    {
        _logger.LogInformation("Received request to remove address ID: {AddressId} for customer ID: {CustomerId}", command.AddressId, command.CustomerId);
        try
        {
            var result = await _customerService.RemoveAddressAsync(command);
            if(result == null)
            {
                _logger.LogWarning("Failed to remove address ID: {AddressId} for customer ID: {CustomerId}", command.AddressId, command.CustomerId);
                return BadRequest(new { Error = "Failed to remove address." });
            }
            return Ok(new { Success = result, Message = "Address removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing address");
            return StatusCode(500, new { Error = "Failed to remove address." });
        }
    }

    /// <summary>
    /// Update customer profile
    /// </summary>
    /// <param name="command">Update profile command</param>
    /// <returns>Success status</returns>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        _logger.LogInformation("Received request to update profile for customer ID: {CustomerId}", command.CustomerId);
        try
        {
            var result = await _customerService.UpdateProfileAsync(command);
            if(result == null)
            {
                _logger.LogWarning("Failed to update profile for customer ID: {CustomerId}", command.CustomerId);
                return BadRequest(new { Error = "Failed to update profile." });
            }
            return Ok(new { Success = result, Message = "Profile updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { Error = "Failed to update profile" });
        }
    }

    /// <summary>
    /// Get customer profile by ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Customer profile details</returns>
    [HttpGet("{customerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCustomerProfile(Guid customerId)
    {
        _logger.LogInformation("Received request to get profile for customer ID: {CustomerId}", customerId);
        try
        {
            var profile = await _customerService.GetCustomerProfileAsync(customerId);
            if(profile == null)
            {
                _logger.LogWarning("Customer profile not found for ID: {CustomerId}", customerId);
                return NotFound(new { Error = "Customer profile not found." });
            }
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer profile for {CustomerId}", customerId);
            return StatusCode(500, new { Error = "Failed to retrieve customer profile" });
        }
    }

    /// <summary>
    /// Search customers with filters
    /// </summary>
    /// <param name="emailPattern">Email pattern filter</param>
    /// <param name="namePattern">Name pattern filter</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of customers matching the criteria</returns>
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? emailPattern = null,
        [FromQuery] string? namePattern = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Received request to search customers with emailPattern: {EmailPattern}, namePattern: {NamePattern}, page: {Page}, pageSize: {PageSize}", 
            emailPattern, namePattern, page, pageSize);
        try
        {
            var customers = await _customerService.SearchCustomersAsync(
                emailPattern,
                namePattern,
                page,
                pageSize);

            if(customers == null || !customers.Any())
            {
                _logger.LogInformation("No customers found matching the search criteria.");
                return NotFound(new { Message = "No customers found matching the search criteria." });
            }
            
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers");
            return StatusCode(500, new { Error = "Failed to search customers" });
        }
    }

    [HttpGet("populate")]
    [Authorize]
    public async Task<IActionResult> PopulateSampleData()
    {
        _logger.LogInformation("Received request to populate sample data for customers");
        try
        {
            var customer = await _customerService.PopulateSampleDataAsync();
            if(customer == null)
            {
                _logger.LogWarning("Failed to populate sample data for customers");
                return BadRequest(new { Error = "Failed to populate sample data." });
            }
            return Ok(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating sample data");
            return StatusCode(500, new { Error = "Failed to populate sample data" });
        }
    }
}