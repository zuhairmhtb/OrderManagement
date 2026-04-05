using OrderManagement.Database.Constants;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Seeds;

public static class ProductSeed
{
    public static List<Product> GetProducts()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99, Currency = Currency.USD, Quantity = 10, Description = "High-performance laptop with 16GB RAM and 512GB SSD." },
            new Product { Id = 2, Name = "Smartphone", Price = 499.99, Currency = Currency.USD, Quantity = 20, Description = "Latest model smartphone with a stunning display and powerful camera." },
            new Product { Id = 3, Name = "Headphones", Price = 199.99, Currency = Currency.USD, Quantity = 15, Description = "Noise-cancelling headphones with superior sound quality." },
            new Product { Id = 4, Name = "Smartwatch", Price = 299.99, Currency = Currency.USD, Quantity = 25, Description = "Stylish smartwatch with fitness tracking and notifications." },
            new Product { Id = 5, Name = "Tablet", Price = 399.99, Currency = Currency.USD, Quantity = 12, Description = "Lightweight tablet with a vibrant display and long battery life." }
        };
    }
}