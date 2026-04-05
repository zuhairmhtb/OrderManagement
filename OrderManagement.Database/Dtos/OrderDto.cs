namespace OrderManagement.Database.Dtos;
public class OrderDto
{
    public Guid Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerEmail { get; set; } = null!;
    public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto BillingAddress { get; set; } = null!;


    public double Vat { get; set; }
    public double ShippingCost { get; set; }
    public double AdditionalCharges { get; set; }
    public string Currency { get; set; } = null!;
    

    /// <summary>
    /// Computed Values - TODO: Currently, the currency is not being used but a CurrencyConverter service can be used 
    /// to convert the price of each product to the order's currency before calculating the cost.
    /// </summary>
    public double Subtotal
    {
        get
        {
            double total = 0;
            foreach (var product in Products)
            {
                total += product.Price * product.Quantity;
            }
            return total;
        }
    }
    public double TotalAmount { 
        get
        {
            var total = Vat + ShippingCost + AdditionalCharges + Subtotal;
            return total;
        } 
    }
}