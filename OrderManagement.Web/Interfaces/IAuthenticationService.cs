using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Constants;
using OrderManagement.Database.Dtos.Customer;

namespace OrderManagement.Web.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponseDto?> LoginAsync(LoginCommand command);

    Task<CustomerProfileDto> RegisterAsync(SignupCommand command, UserRole role);
    Task<bool> UpdatePasswordAsync(UpdatePasswordCommand command);
}