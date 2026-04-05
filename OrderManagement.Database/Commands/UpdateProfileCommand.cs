using System.ComponentModel.DataAnnotations;
using OrderManagement.Database.Dtos;

namespace OrderManagement.Database.Commands;

public class UpdateProfileDto
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public AddressDto? Address { get; set; }
}
