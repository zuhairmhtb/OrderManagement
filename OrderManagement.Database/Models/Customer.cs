using System.ComponentModel.DataAnnotations;
using OrderManagement.Database.Constants;
namespace OrderManagement.Database.Models;

public class Customer
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = null!;
    
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Password { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;

    [MaxLength(50)]
    public string? FirstName { get; set; }
    
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    // Navigation property
    public IEnumerable<Address> Addresses { get; set; } = new List<Address>();
    
}