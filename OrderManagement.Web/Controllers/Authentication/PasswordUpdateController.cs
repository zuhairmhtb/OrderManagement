using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Web.Interfaces;

namespace OrderManagement.Web.Controllers.Authentication;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PasswordUpdateController : ControllerBase
{
    private readonly ILogger<PasswordUpdateController> _logger;
    private readonly IAuthenticationService _authenticationService;
    public PasswordUpdateController(ILogger<PasswordUpdateController> logger, IAuthenticationService authenticationService)
    {
        _logger = logger;
        _authenticationService = authenticationService;
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdatePasswordCommand command)
    {
        try
        {
            // Signup logic would go here, but is out of scope for this task.
            var result = await _authenticationService.UpdatePasswordAsync(command);
            if(result)
                return Ok(new { Message = "Password updated successfully." });
            else
                return BadRequest(new { Error = "Password update failed." });
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error during signup");
            return StatusCode(500, new { Error = "Signup failed" });
        }
        
    }
}