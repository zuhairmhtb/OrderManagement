using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    
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
    
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int Quantity { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
}