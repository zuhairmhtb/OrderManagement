using OrderManagement.Database.Constants;
using OrderManagement.Database.Models;

namespace OrderManagement.Database.Seeds;

public static class CustomerSeed
{
    private static readonly string[] FirstNames = 
    {
        "John", "Jane", "Michael", "Sarah", "David", "Emma", "Robert", "Lisa",
        "William", "Jennifer", "James", "Michelle", "Christopher", "Jessica", "Daniel", "Ashley",
        "Matthew", "Amanda", "Anthony", "Melissa", "Mark", "Deborah", "Steven", "Stephanie",
        "Paul", "Dorothy", "Andrew", "Carol", "Joshua", "Ruth", "Kenneth", "Sharon"
    };

    private static readonly string[] LastNames = 
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young"
    };

    private static readonly string[] EmailDomains = 
    {
        "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "protonmail.com",
        "icloud.com", "aol.com", "mail.com", "zoho.com", "fastmail.com"
    };

    public static List<Customer> GetCustomers(int total = 5)
    {
        var random = new Random(12345); // Fixed seed for consistent results
        var customers = new List<Customer>();

        for (int i = 1; i <= total; i++)
        {
            var firstName = FirstNames[random.Next(FirstNames.Length)];
            var lastName = LastNames[random.Next(LastNames.Length)];
            var emailDomain = EmailDomains[random.Next(EmailDomains.Length)];
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(100, 999)}@{emailDomain}";
            
            // Generate phone number in format: +1-XXX-XXX-XXXX
            var phoneNumber = $"+1-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}";
            
            // Generate password with mix of letters and numbers
            var password = $"{firstName.Substring(0, Math.Min(3, firstName.Length))}{lastName.Substring(0, Math.Min(3, lastName.Length))}{random.Next(100, 999)}!";

            customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                Email = email,
                Password = password,
                Role = UserRole.Customer,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
            });
        }

        return customers;
    }

    public static List<Customer> GetStaticCustomers()
    {
        return new List<Customer>
        {
            new Customer { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Email = "john.smith@gmail.com", Password = "JohSmi123!", Role = UserRole.Customer, FirstName = "John", LastName = "Smith", PhoneNumber = "+1-555-123-4567" },
            new Customer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Email = "sarah.johnson@yahoo.com", Password = "SarJoh456!", Role = UserRole.Customer, FirstName = "Sarah", LastName = "Johnson", PhoneNumber = "+1-555-234-5678" },
            new Customer { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Email = "michael.brown@hotmail.com", Password = "MicBro789!", Role = UserRole.Customer, FirstName = "Michael", LastName = "Brown", PhoneNumber = "+1-555-345-6789" },
            new Customer { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Email = "emma.davis@outlook.com", Password = "EmmDav101!", Role = UserRole.Customer, FirstName = "Emma", LastName = "Davis", PhoneNumber = "+1-555-456-7890" },
            new Customer { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Email = "david.wilson@protonmail.com", Password = "DavWil202!", Role = UserRole.Customer, FirstName = "David", LastName = "Wilson", PhoneNumber = "+1-555-567-8901" },
            new Customer { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Email = "jennifer.garcia@icloud.com", Password = "JenGar303!", Role = UserRole.Customer, FirstName = "Jennifer", LastName = "Garcia", PhoneNumber = "+1-555-678-9012" },
            new Customer { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Email = "robert.martinez@aol.com", Password = "RobMar404!", Role = UserRole.Customer, FirstName = "Robert", LastName = "Martinez", PhoneNumber = "+1-555-789-0123" },
            new Customer { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Email = "lisa.anderson@mail.com", Password = "LisAnd505!", Role = UserRole.Customer, FirstName = "Lisa", LastName = "Anderson", PhoneNumber = "+1-555-890-1234" },
            new Customer { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Email = "william.taylor@zoho.com", Password = "WilTay606!", Role = UserRole.Customer, FirstName = "William", LastName = "Taylor", PhoneNumber = "+1-555-901-2345" },
            new Customer { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Email = "jessica.thomas@fastmail.com", Password = "JesTho707!", Role = UserRole.Customer, FirstName = "Jessica", LastName = "Thomas", PhoneNumber = "+1-555-012-3456" }
        };
    }
}