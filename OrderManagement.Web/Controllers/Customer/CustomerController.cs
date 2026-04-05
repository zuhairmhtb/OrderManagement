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
    public async Task<IActionResult> AddAddress([FromBody] AddAddressCommand command)
    {
        try
        {
            var result = await _customerService.AddAddressAsync(command);
            return Ok(new { Success = result, Message = "Address added successfully" });
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
    public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressCommand command)
    {
        try
        {
            var result = await _customerService.UpdateAddressAsync(command);
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
    public async Task<IActionResult> RemoveAddress([FromBody] RemoveAddressCommand command)
    {
        try
        {
            var result = await _customerService.RemoveAddressAsync(command);
            return Ok(new { Success = result, Message = "Address removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing address");
            return StatusCode(500, new { Error = "Failed to remove address" });
        }
    }

    /// <summary>
    /// Update customer profile
    /// </summary>
    /// <param name="command">Update profile command</param>
    /// <returns>Success status</returns>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        try
        {
            var result = await _customerService.UpdateProfileAsync(command);
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
    public async Task<IActionResult> GetCustomerProfile(Guid customerId)
    {
        try
        {
            var profile = await _customerService.GetCustomerProfileAsync(customerId);
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
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? emailPattern = null,
        [FromQuery] string? namePattern = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var customers = await _customerService.SearchCustomersAsync(
                emailPattern,
                namePattern,
                page,
                pageSize);
            
            return Ok(customers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers");
            return StatusCode(500, new { Error = "Failed to search customers" });
        }
    }
}