using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Dtos.Customer;

namespace OrderManagement.Web.Interfaces;

public interface ICustomerService
{
    Task<AddressDto> AddAddressAsync(AddAddressCommand command);
    Task<AddressDto> UpdateAddressAsync(UpdateAddressCommand command);
    Task<bool> RemoveAddressAsync(RemoveAddressCommand command);
    Task<CustomerProfileDto> UpdateProfileAsync(UpdateProfileCommand command);
}