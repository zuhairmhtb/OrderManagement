using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Models;

public class Order
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Order Details
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeliveryDate { get; set; }
    
    [Required]
    [MaxLength(50)]
    public OrderStatus OrderStatus { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "VAT must be non-negative")]
    [Column(TypeName = "decimal(18,2)")]
    public double Vat { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Shipping cost must be non-negative")]
    [Column(TypeName = "decimal(18,2)")]
    public double ShippingCost { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Additional charges must be non-negative")]
    [Column(TypeName = "decimal(18,2)")]
    public double AdditionalCharges { get; set; } = 0;
    
    [Required]
    [MaxLength(3)]
    public Currency Currency { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Subtotal must be non-negative")]
    [Column(TypeName = "decimal(18,2)")]
    public double Subtotal { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative")]
    [Column(TypeName = "decimal(18,2)")]
    public double TotalAmount { get; set; }


    // Customer Details
    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public string CustomerEmail { get; set; } = null!;
    [Required]
    public string CustomerContactNumber { get; set; } = null!;

    // Shipping Address
    [Required]
    [MaxLength(200)]
    public string ShippingStreet { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string ShippingCity { get; set; } = null!;
    
    [Required]
    [MaxLength(20)]
    public string ShippingPostalCode { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string ShippingCountry { get; set; } = null!;
    
    [MaxLength(100)]
    public string? ShippingState { get; set; }

    // Billing Address
    [Required]
    [MaxLength(200)]
    public string BillingStreet { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string BillingCity { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string BillingPostalCode { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string BillingCountry { get; set; } = null!;
    
    [MaxLength(100)]
    public string? BillingState { get; set; }

    // Navigation property
    public List<PurchasedProduct> Products { get; set; } = new List<PurchasedProduct>();

    

    /// <summary>
    /// Computed Values - TODO: Currently, the currency is not being used but a CurrencyConverter service can be used 
    /// to convert the price of each product to the order's currency before calculating the cost.
    /// </summary>
    // public double Subtotal
    // {
    //     get
    //     {
    //         double total = 0;
    //         foreach (var product in Products)
    //         {
    //             total += product.Price * product.Quantity;
    //         }
    //         return total;
    //     }
    // }
    // public double TotalAmount { 
    //     get
    //     {
    //         var total = Vat + ShippingCost + AdditionalCharges + Subtotal;
    //         return total;
    //     } 
    // }
}