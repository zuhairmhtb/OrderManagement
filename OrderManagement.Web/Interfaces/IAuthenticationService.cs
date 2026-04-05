using OrderManagement.Database.Commands.Authentication;

namespace OrderManagement.Web.Interfaces;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(LoginCommand command);
    
    Task<bool> RegisterAsync(SignupCommand command);
    Task<bool> UpdatePasswordAsync(UpdatePasswordCommand command);
}