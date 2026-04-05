namespace OrderManagement.Database.Dtos.Product;

public class ProductDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public double Price { get; set; }
    public int StockQuantity { get; set; }
}