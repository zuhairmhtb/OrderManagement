using Microsoft.AspNetCore.Mvc;
using OrderManagement.Database.Commands.Authentication;
using OrderManagement.Database.Commands.Customer;
using OrderManagement.Database.Dtos.Customer;

namespace OrderManagement.Web.Interfaces;

public interface ICustomerService
{
    Task<CustomerProfileDto> AddAddressAsync(AddAddressCommand command);
    Task<CustomerProfileDto> UpdateAddressAsync(UpdateAddressCommand command);
    Task<CustomerProfileDto> RemoveAddressAsync(RemoveAddressCommand command);
    Task<CustomerProfileDto> UpdateProfileAsync(UpdateProfileCommand command);

    Task<CustomerProfileDto> GetCustomerProfileAsync(Guid customerId);
    Task<IEnumerable<CustomerProfileDto>> SearchCustomersAsync(
        string? emailPattern = null,
        string? namePattern = null,
        int page = 1,
        int pageSize = 20
    );

    Task<CustomerProfileDto> PopulateSampleDataAsync();
}