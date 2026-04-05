using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Dtos.Customer;

namespace OrderManagement.Web.Interfaces;

public interface ICustomerService
{
    Task<bool> AddAddressAsync(AddAddressCommand command);
    Task<bool> UpdateAddressAsync(UpdateAddressCommand command);
    Task<bool> RemoveAddressAsync(RemoveAddressCommand command);
    Task<bool> UpdateProfileAsync(UpdateProfileCommand command);

    Task<CustomerProfileDto> GetCustomerProfileAsync(Guid customerId);
    Task<IEnumerable<CustomerProfileDto>> SearchCustomersAsync(
        string? emailPattern = null,
        string? namePattern = null,
        int page = 1,
        int pageSize = 20
    );
}