
using OrderManagement.Database.Constants;

namespace OrderManagement.Database.Dtos;

public class ProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }
    public Currency Currency { get; set; }
    public int Quantity { get; set; }
    public string? Description { get; set; }
}