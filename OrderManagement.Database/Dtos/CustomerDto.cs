

using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Dtos;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }

    public AddressDto? Address { get; set; }
}