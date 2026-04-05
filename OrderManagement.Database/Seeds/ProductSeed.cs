using OrderManagement.Database.Constants;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Seeds;

public static class ProductSeed
{
    private static readonly string[] ProductNames = 
    {
        "Laptop", "Smartphone", "Headphones", "Smartwatch", "Tablet", 
        "Monitor", "Keyboard", "Mouse", "Speaker", "Camera", 
        "Gaming Console", "Hard Drive", "USB Cable", "Charger", "Router",
        "Webcam", "Microphone", "Graphics Card", "Memory Card", "Power Bank"
    };

    private static readonly string[] ProductDescriptions = 
    {
        "High-quality electronic device with premium features.",
        "Advanced technology product designed for modern users.",
        "Innovative gadget with cutting-edge functionality.",
        "Professional-grade equipment for enhanced productivity.",
        "Sleek and stylish device with superior performance.",
        "Durable product built to last with excellent reliability.",
        "User-friendly device with intuitive design and features.",
        "Compact and portable solution for everyday needs."
    };

    public static List<Product> GetProducts(int total = 5)
    {
        var random = new Random(12345); // Fixed seed for consistent results
        var products = new List<Product>();

        for (int i = 1; i <= total; i++)
        {
            var productName = ProductNames[random.Next(ProductNames.Length)];
            var description = ProductDescriptions[random.Next(ProductDescriptions.Length)];
            var price = Math.Round(random.NextDouble() * 1500 + 50, 2); // Price between $50 and $1550
            var quantity = random.Next(5, 51); // Quantity between 5 and 50

            products.Add(new Product
            {
                Id = i,
                Name = productName,
                Price = price,
                Currency = Currency.USD,
                Quantity = quantity,
                Description = description
            });
        }

        return products;
    }

    public static List<Product> GetStaticProducts()
    {
        return new List<Product>
        {
            new Product { Id = 1, Name = "MacBook Pro 16\"", Price = 2499.99, Currency = Currency.USD, Quantity = 15, Description = "High-performance laptop with M2 Pro chip, 16GB RAM and 512GB SSD." },
            new Product { Id = 2, Name = "iPhone 15 Pro", Price = 999.99, Currency = Currency.USD, Quantity = 25, Description = "Latest iPhone with A17 Pro chip, titanium design, and advanced camera system." },
            new Product { Id = 3, Name = "Sony WH-1000XM5", Price = 399.99, Currency = Currency.USD, Quantity = 20, Description = "Industry-leading noise canceling headphones with 30-hour battery life." },
            new Product { Id = 4, Name = "Apple Watch Series 9", Price = 399.99, Currency = Currency.USD, Quantity = 30, Description = "Advanced smartwatch with health monitoring and fitness tracking capabilities." },
            new Product { Id = 5, Name = "iPad Pro 12.9\"", Price = 1099.99, Currency = Currency.USD, Quantity = 18, Description = "Powerful tablet with M2 chip, Liquid Retina XDR display, and Apple Pencil support." },
            new Product { Id = 6, Name = "Dell UltraSharp 27\" 4K Monitor", Price = 649.99, Currency = Currency.USD, Quantity = 12, Description = "Professional 4K monitor with USB-C connectivity and color accuracy certification." },
            new Product { Id = 7, Name = "Logitech MX Master 3S", Price = 99.99, Currency = Currency.USD, Quantity = 40, Description = "Wireless mouse with ultra-fast scrolling and customizable buttons for productivity." },
            new Product { Id = 8, Name = "Mechanical Gaming Keyboard", Price = 149.99, Currency = Currency.USD, Quantity = 35, Description = "RGB mechanical keyboard with Cherry MX switches and programmable macros." },
            new Product { Id = 9, Name = "Bose SoundLink Revolve+", Price = 299.99, Currency = Currency.USD, Quantity = 22, Description = "360-degree Bluetooth speaker with deep bass and 16-hour battery life." },
            new Product { Id = 10, Name = "Canon EOS R6 Mark II", Price = 2499.99, Currency = Currency.USD, Quantity = 8, Description = "Full-frame mirrorless camera with 24.2MP sensor and advanced autofocus system." }
        };
    }
}