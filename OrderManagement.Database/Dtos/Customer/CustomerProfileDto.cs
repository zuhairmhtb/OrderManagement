using System.ComponentModel.DataAnnotations;
using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Dtos.Customer;

public class CustomerProfileDto
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    [MaxLength(50)]
    public string? FirstName { get; set; }
    
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public IEnumerable<AddressDto>? Addresses { get; set; } = Enumerable.Empty<AddressDto>();
}