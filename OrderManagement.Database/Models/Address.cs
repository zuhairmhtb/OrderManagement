using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderManagement.Database.Models;

public class Address
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Street { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = null!;
    
    [MaxLength(100)]
    public string? State { get; set; }

    // Navigation property
    [Required]
    public Guid CustomerId { get; set; }
    [JsonIgnore]
    public Customer? CustomerInfo { get; set;}

    [Timestamp]
    public byte[] RowVersion { get; set; } = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
}