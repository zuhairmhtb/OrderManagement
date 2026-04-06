using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Authentication;


[ApiController]
[Route("api/[controller]")]
public class SignupController : ControllerBase
{
    private readonly ILogger<SignupController> _logger;
    private readonly IAuthenticationService _authenticationService;
    public SignupController(ILogger<SignupController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    [HttpPost("admin")]
    public async Task<IActionResult> SignupAdmin(SignupCommand command)
    {
        _logger.LogInformation("Received admin signup request for email: {Email}", command.Email);
        try
        {
            // Signup logic would go here, but is out of scope for this task.
            var result = await _authenticationService.RegisterAsync(command, Database.Constants.UserRole.Admin);
            if(result != null)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Admin signup failed for email: {Email}", command.Email);
                return BadRequest(new { Error = "Signup failed." });   
            }
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin signup");
            return StatusCode(500, new { Error = "Signup failed" });
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Signup(SignupCommand command)
    {
        _logger.LogInformation("Received customer signup request for email: {Email}", command.Email);
        try
        {
            // Signup logic would go here, but is out of scope for this task.
            var result = await _authenticationService.RegisterAsync(command, Database.Constants.UserRole.Customer);
            if(result != null)
            {
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Customer signup failed for email: {Email}", command.Email);
                return BadRequest(new { Error = "Signup failed." });   
            }
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup");
            return StatusCode(500, new { Error = "Signup failed" });
        }
        
    }
}