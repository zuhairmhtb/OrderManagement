using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Models;

public class PurchasedProduct
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Column(TypeName = "decimal(18,2)")]
    public double Price { get; set; }
    
    [Required]
    [MaxLength(3)]
    public Currency Currency { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    // Navigation property
    [Required]
    public Guid OrderId { get; set; }

    [JsonIgnore]
    public Order? OrderInfo { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
}