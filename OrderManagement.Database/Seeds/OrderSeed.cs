using OrderManagement.Database.Constants;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Seeds;

public static class OrderSeed
{
    private static readonly string[] Streets = 
    {
        "Main Street", "Oak Avenue", "Pine Road", "Cedar Lane", "Maple Drive",
        "First Avenue", "Second Street", "Park Boulevard", "Elm Street", "Washington Ave",
        "Lincoln Road", "Jefferson Street", "Madison Avenue", "Hamilton Drive", "Franklin Lane",
        "Church Street", "School Road", "Mill Street", "High Street", "Market Street"
    };

    private static readonly string[] Cities = 
    {
        "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia",
        "San Antonio", "San Diego", "Dallas", "San Jose", "Austin", "Jacksonville",
        "Fort Worth", "Columbus", "Charlotte", "Seattle", "Denver", "Boston",
        "Nashville", "Baltimore", "Portland", "Miami", "Atlanta", "Tampa"
    };

    private static readonly string[] States = 
    {
        "CA", "TX", "FL", "NY", "PA", "IL", "OH", "GA", "NC", "MI",
        "NJ", "VA", "WA", "AZ", "MA", "TN", "IN", "MO", "MD", "WI"
    };

    private static readonly string[] Countries = 
    {
        "United States", "Canada", "United Kingdom", "Australia", "Germany",
        "France", "Netherlands", "Sweden", "Norway", "Denmark"
    };

    public static List<Order> GetOrders(int total = 5)
    {
        var random = new Random(12345); // Fixed seed for consistent results
        var orders = new List<Order>();

        for (int i = 1; i <= total; i++)
        {
            // Generate random dates
            var orderDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));
            var deliveryDate = orderDate.AddDays(random.Next(1, 14));

            // Generate random amounts
            var subtotal = Math.Round(random.NextDouble() * 1000 + 50, 2);
            var vat = Math.Round(subtotal * 0.1, 2); // 10% VAT
            var shippingCost = Math.Round(random.NextDouble() * 50 + 5, 2);
            var additionalCharges = Math.Round(random.NextDouble() * 20, 2);
            var totalAmount = subtotal + vat + shippingCost + additionalCharges;

            // Generate random addresses
            var shippingStreet = $"{random.Next(1, 9999)} {Streets[random.Next(Streets.Length)]}";
            var shippingCity = Cities[random.Next(Cities.Length)];
            var shippingState = States[random.Next(States.Length)];
            var shippingPostalCode = random.Next(10000, 99999).ToString();
            var shippingCountry = Countries[random.Next(Countries.Length)];

            var billingStreet = $"{random.Next(1, 9999)} {Streets[random.Next(Streets.Length)]}";
            var billingCity = Cities[random.Next(Cities.Length)];
            var billingState = States[random.Next(States.Length)];
            var billingPostalCode = random.Next(10000, 99999).ToString();
            var billingCountry = Countries[random.Next(Countries.Length)];

            // Generate customer info
            var customerEmail = $"customer{random.Next(100, 999)}@example.com";
            var customerContact = $"+1-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}";

            // Random order status
            var orderStatuses = Enum.GetValues<OrderStatus>();
            var randomStatus = orderStatuses[random.Next(orderStatuses.Length)];

            orders.Add(new Order
            {
                Id = Guid.NewGuid(),
                OrderDate = orderDate,
                DeliveryDate = deliveryDate,
                OrderStatus = randomStatus,
                Vat = vat,
                ShippingCost = shippingCost,
                AdditionalCharges = additionalCharges,
                Currency = Currency.USD,
                Subtotal = subtotal,
                TotalAmount = totalAmount,
                CustomerId = Guid.NewGuid(),
                CustomerEmail = customerEmail,
                CustomerContactNumber = customerContact,
                ShippingStreet = shippingStreet,
                ShippingCity = shippingCity,
                ShippingState = shippingState,
                ShippingPostalCode = shippingPostalCode,
                ShippingCountry = shippingCountry,
                BillingStreet = billingStreet,
                BillingCity = billingCity,
                BillingState = billingState,
                BillingPostalCode = billingPostalCode,
                BillingCountry = billingCountry
            });
        }

        return orders;
    }

    public static List<Order> GetStaticOrders()
    {
        return new List<Order>
        {
            new Order
            {
                Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                OrderDate = new DateTime(2026, 3, 1, 10, 30, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 3, 8, 14, 0, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Delivered,
                Vat = 149.99,
                ShippingCost = 15.99,
                AdditionalCharges = 5.00,
                Currency = Currency.USD,
                Subtotal = 1499.99,
                TotalAmount = 1670.97,
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerEmail = "john.smith@gmail.com",
                CustomerContactNumber = "+1-555-123-4567",
                ShippingStreet = "123 Main Street",
                ShippingCity = "New York",
                ShippingState = "NY",
                ShippingPostalCode = "10001",
                ShippingCountry = "United States",
                BillingStreet = "123 Main Street",
                BillingCity = "New York",
                BillingState = "NY",
                BillingPostalCode = "10001",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
                OrderDate = new DateTime(2026, 3, 5, 15, 45, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 3, 12, 16, 30, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Shipped,
                Vat = 89.99,
                ShippingCost = 12.99,
                AdditionalCharges = 2.50,
                Currency = Currency.USD,
                Subtotal = 899.99,
                TotalAmount = 1005.47,
                CustomerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CustomerEmail = "sarah.johnson@yahoo.com",
                CustomerContactNumber = "+1-555-234-5678",
                ShippingStreet = "456 Oak Avenue",
                ShippingCity = "Los Angeles",
                ShippingState = "CA",
                ShippingPostalCode = "90210",
                ShippingCountry = "United States",
                BillingStreet = "456 Oak Avenue",
                BillingCity = "Los Angeles",
                BillingState = "CA",
                BillingPostalCode = "90210",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("cccccccc-3333-3333-3333-333333333333"),
                OrderDate = new DateTime(2026, 3, 10, 9, 15, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 3, 17, 11, 45, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Processing,
                Vat = 39.99,
                ShippingCost = 8.99,
                AdditionalCharges = 0.00,
                Currency = Currency.USD,
                Subtotal = 399.99,
                TotalAmount = 448.97,
                CustomerId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CustomerEmail = "michael.brown@hotmail.com",
                CustomerContactNumber = "+1-555-345-6789",
                ShippingStreet = "789 Pine Road",
                ShippingCity = "Chicago",
                ShippingState = "IL",
                ShippingPostalCode = "60601",
                ShippingCountry = "United States",
                BillingStreet = "789 Pine Road",
                BillingCity = "Chicago",
                BillingState = "IL",
                BillingPostalCode = "60601",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("dddddddd-4444-4444-4444-444444444444"),
                OrderDate = new DateTime(2026, 3, 15, 14, 20, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 3, 22, 10, 0, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Pending,
                Vat = 109.99,
                ShippingCost = 19.99,
                AdditionalCharges = 7.50,
                Currency = Currency.USD,
                Subtotal = 1099.99,
                TotalAmount = 1237.47,
                CustomerId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                CustomerEmail = "emma.davis@outlook.com",
                CustomerContactNumber = "+1-555-456-7890",
                ShippingStreet = "321 Cedar Lane",
                ShippingCity = "Houston",
                ShippingState = "TX",
                ShippingPostalCode = "77001",
                ShippingCountry = "United States",
                BillingStreet = "321 Cedar Lane",
                BillingCity = "Houston",
                BillingState = "TX",
                BillingPostalCode = "77001",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("eeeeeeee-5555-5555-5555-555555555555"),
                OrderDate = new DateTime(2026, 3, 20, 11, 30, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 3, 27, 15, 30, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Canceled,
                Vat = 64.99,
                ShippingCost = 9.99,
                AdditionalCharges = 3.25,
                Currency = Currency.USD,
                Subtotal = 649.99,
                TotalAmount = 728.22,
                CustomerId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                CustomerEmail = "david.wilson@protonmail.com",
                CustomerContactNumber = "+1-555-567-8901",
                ShippingStreet = "654 Maple Drive",
                ShippingCity = "Phoenix",
                ShippingState = "AZ",
                ShippingPostalCode = "85001",
                ShippingCountry = "United States",
                BillingStreet = "654 Maple Drive",
                BillingCity = "Phoenix",
                BillingState = "AZ",
                BillingPostalCode = "85001",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("ffffffff-6666-6666-6666-666666666666"),
                OrderDate = new DateTime(2026, 3, 25, 16, 45, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 4, 1, 12, 15, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Returned,
                Vat = 29.99,
                ShippingCost = 6.99,
                AdditionalCharges = 1.50,
                Currency = Currency.USD,
                Subtotal = 299.99,
                TotalAmount = 338.47,
                CustomerId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                CustomerEmail = "jennifer.garcia@icloud.com",
                CustomerContactNumber = "+1-555-678-9012",
                ShippingStreet = "987 First Avenue",
                ShippingCity = "Philadelphia",
                ShippingState = "PA",
                ShippingPostalCode = "19101",
                ShippingCountry = "United States",
                BillingStreet = "987 First Avenue",
                BillingCity = "Philadelphia",
                BillingState = "PA",
                BillingPostalCode = "19101",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("gggggggg-7777-7777-7777-777777777777"),
                OrderDate = new DateTime(2026, 3, 28, 13, 10, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 4, 4, 9, 45, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Delivered,
                Vat = 14.99,
                ShippingCost = 4.99,
                AdditionalCharges = 0.75,
                Currency = Currency.USD,
                Subtotal = 149.99,
                TotalAmount = 170.72,
                CustomerId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                CustomerEmail = "robert.martinez@aol.com",
                CustomerContactNumber = "+1-555-789-0123",
                ShippingStreet = "147 Second Street",
                ShippingCity = "San Antonio",
                ShippingState = "TX",
                ShippingPostalCode = "78201",
                ShippingCountry = "United States",
                BillingStreet = "147 Second Street",
                BillingCity = "San Antonio",
                BillingState = "TX",
                BillingPostalCode = "78201",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("hhhhhhhh-8888-8888-8888-888888888888"),
                OrderDate = new DateTime(2026, 3, 30, 8, 25, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 4, 6, 14, 20, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Processing,
                Vat = 249.99,
                ShippingCost = 24.99,
                AdditionalCharges = 12.00,
                Currency = Currency.USD,
                Subtotal = 2499.99,
                TotalAmount = 2786.97,
                CustomerId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                CustomerEmail = "lisa.anderson@mail.com",
                CustomerContactNumber = "+1-555-890-1234",
                ShippingStreet = "258 Park Boulevard",
                ShippingCity = "San Diego",
                ShippingState = "CA",
                ShippingPostalCode = "92101",
                ShippingCountry = "United States",
                BillingStreet = "258 Park Boulevard",
                BillingCity = "San Diego",
                BillingState = "CA",
                BillingPostalCode = "92101",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("iiiiiiii-9999-9999-9999-999999999999"),
                OrderDate = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 4, 8, 16, 0, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Shipped,
                Vat = 99.99,
                ShippingCost = 14.99,
                AdditionalCharges = 4.25,
                Currency = Currency.USD,
                Subtotal = 999.99,
                TotalAmount = 1119.22,
                CustomerId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                CustomerEmail = "william.taylor@zoho.com",
                CustomerContactNumber = "+1-555-901-2345",
                ShippingStreet = "369 Elm Street",
                ShippingCity = "Dallas",
                ShippingState = "TX",
                ShippingPostalCode = "75201",
                ShippingCountry = "United States",
                BillingStreet = "369 Elm Street",
                BillingCity = "Dallas",
                BillingState = "TX",
                BillingPostalCode = "75201",
                BillingCountry = "United States"
            },
            new Order
            {
                Id = Guid.Parse("jjjjjjjj-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                OrderDate = new DateTime(2026, 4, 3, 17, 30, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 4, 10, 13, 45, 0, DateTimeKind.Utc),
                OrderStatus = OrderStatus.Pending,
                Vat = 39.99,
                ShippingCost = 7.99,
                AdditionalCharges = 2.00,
                Currency = Currency.USD,
                Subtotal = 399.99,
                TotalAmount = 449.97,
                CustomerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CustomerEmail = "jessica.thomas@fastmail.com",
                CustomerContactNumber = "+1-555-012-3456",
                ShippingStreet = "741 Washington Ave",
                ShippingCity = "San Jose",
                ShippingState = "CA",
                ShippingPostalCode = "95101",
                ShippingCountry = "United States",
                BillingStreet = "741 Washington Ave",
                BillingCity = "San Jose",
                BillingState = "CA",
                BillingPostalCode = "95101",
                BillingCountry = "United States"
            }
        };
    }
}