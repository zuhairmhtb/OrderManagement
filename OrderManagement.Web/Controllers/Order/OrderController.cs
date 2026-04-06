using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Order;
using OrderManagement.Database.Constants;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Order;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Place a new order
    /// </summary>
    /// <param name="command">Order placement command</param>
    /// <returns>Order status after placement</returns>
    [HttpPost("place")]
    [Authorize]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
    {
        _logger.LogInformation("Received request to place order for customer ID: {CustomerId}", command.CustomerId);
        try
        {
            var orderInformation = await _orderService.PlaceOrderAsync(command);
            return Ok(new { Order = orderInformation, Message = "Order placed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing order");
            return StatusCode(500, new { Error = "Failed to place order" });
        }
    }

    /// <summary>
    /// Get order details by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderDetails(Guid orderId)
    {
        _logger.LogInformation("Received request to get details for order ID: {OrderId}", orderId);
        try
        {
            var orderDetails = await _orderService.GetOrderDetailsAsync(orderId);
            if(orderDetails == null)
            {
                _logger.LogWarning("Order details not found for ID: {OrderId}", orderId);
                return NotFound(new { Error = "Order details not found." });
            }
            return Ok(orderDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details for {OrderId}", orderId);
            return StatusCode(500, new { Error = "Failed to retrieve order details" });
        }
    }

    /// <summary>
    /// Get order status by ID
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Order status</returns>
    [HttpGet("{orderId}/status")]
    [Authorize]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        _logger.LogInformation("Received request to get status for order ID: {OrderId}", orderId);
        try
        {
            var orderStatus = await _orderService.GetOrderStatusAsync(orderId);
            return Ok(new { Status = orderStatus.ToString() });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order status for {OrderId}", orderId);
            return StatusCode(500, new { Error = "Failed to retrieve order status" });
        }
    }

    /// <summary>
    /// Search orders with filters
    /// </summary>
    /// <param name="placedAtStartRange">Start date for order placement filter</param>
    /// <param name="placedAtEndRange">End date for order placement filter</param>
    /// <param name="deliveredOnStartRange">Start date for delivery filter</param>
    /// <param name="deliveredOnEndRange">End date for delivery filter</param>
    /// <param name="customerId">Customer ID filter</param>
    /// <param name="customerEmailPattern">Customer email pattern filter</param>
    /// <param name="status">Order status filter</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <returns>List of orders matching the criteria</returns>
    [HttpGet("search")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchOrders(
        [FromQuery] DateTime? placedAtStartRange = null,
        [FromQuery] DateTime? placedAtEndRange = null,
        [FromQuery] DateTime? deliveredOnStartRange = null,
        [FromQuery] DateTime? deliveredOnEndRange = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] string? customerEmailPattern = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Received request to search orders with filters - placedAtStartRange: {PlacedAtStartRange}, placedAtEndRange: {PlacedAtEndRange}, deliveredOnStartRange: {DeliveredOnStartRange}, deliveredOnEndRange: {DeliveredOnEndRange}, customerId: {CustomerId}, customerEmailPattern: {CustomerEmailPattern}, status: {Status}, page: {Page}, pageSize: {PageSize}",
            placedAtStartRange, placedAtEndRange, deliveredOnStartRange, deliveredOnEndRange, customerId, customerEmailPattern, status, page, pageSize);
        try
        {
            var orders = await _orderService.SearchOrdersAsync(
                placedAtStartRange,
                placedAtEndRange,
                deliveredOnStartRange,
                deliveredOnEndRange,
                customerId,
                customerEmailPattern,
                status,
                page,
                pageSize);

            if(orders == null || !orders.Any())
            {
                _logger.LogInformation("No orders found matching the search criteria.");
                return NotFound(new { Message = "No orders found matching the search criteria." });
            }
            
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching orders");
            return StatusCode(500, new { Error = "Failed to search orders" });
        }
    }

    [HttpGet("populate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PopulateSampleData()
    {
        _logger.LogInformation("Received request to populate sample data for orders");
        try
        {
            // Create simulated data and populate the database
            var order = await _orderService.PopulateSampleDataAsync(null);
            if(order == null)
            {
                _logger.LogWarning("Failed to populate sample data for orders");
                return BadRequest(new { Error = "Failed to populate sample data." });
            }
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating sample data");
            return StatusCode(500, new { Error = "Failed to populate sample data" });
        }
    }
}
