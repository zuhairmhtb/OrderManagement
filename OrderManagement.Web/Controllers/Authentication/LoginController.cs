using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private readonly IAuthenticationService _authenticationService;

    public LoginController(ILogger<LoginController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT bearer token.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        _logger.LogInformation("Received login request for email: {Email}", command.Email);
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authenticationService.LoginAsync(command);

            if (result == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", command.Email);
                return Unauthorized(new { Error = "Invalid email or password." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", command.Email);
            return StatusCode(500, new { Error = "An unexpected error occurred. Please try again later." });
        }
    }
}
