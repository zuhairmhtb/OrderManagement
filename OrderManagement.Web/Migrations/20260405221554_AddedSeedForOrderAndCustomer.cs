using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeedForOrderAndCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "Password", "PhoneNumber", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "john.smith@gmail.com", "John", "Smith", "JohSmi123!", "+1-555-123-4567", 0 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "sarah.johnson@yahoo.com", "Sarah", "Johnson", "SarJoh456!", "+1-555-234-5678", 0 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "michael.brown@hotmail.com", "Michael", "Brown", "MicBro789!", "+1-555-345-6789", 0 },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "emma.davis@outlook.com", "Emma", "Davis", "EmmDav101!", "+1-555-456-7890", 0 },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "david.wilson@protonmail.com", "David", "Wilson", "DavWil202!", "+1-555-567-8901", 0 },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "jennifer.garcia@icloud.com", "Jennifer", "Garcia", "JenGar303!", "+1-555-678-9012", 0 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "robert.martinez@aol.com", "Robert", "Martinez", "RobMar404!", "+1-555-789-0123", 0 },
                    { new Guid("88888888-8888-8888-8888-888888888888"), "lisa.anderson@mail.com", "Lisa", "Anderson", "LisAnd505!", "+1-555-890-1234", 0 },
                    { new Guid("99999999-9999-9999-9999-999999999999"), "william.taylor@zoho.com", "William", "Taylor", "WilTay606!", "+1-555-901-2345", 0 },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "jessica.thomas@fastmail.com", "Jessica", "Thomas", "JesTho707!", "+1-555-012-3456", 0 }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "High-performance laptop with M2 Pro chip, 16GB RAM and 512GB SSD.", "MacBook Pro 16\"", 2499.99m, 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Latest iPhone with A17 Pro chip, titanium design, and advanced camera system.", "iPhone 15 Pro", 999.99m, 25 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Industry-leading noise canceling headphones with 30-hour battery life.", "Sony WH-1000XM5", 399.99m, 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Advanced smartwatch with health monitoring and fitness tracking capabilities.", "Apple Watch Series 9", 399.99m, 30 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Powerful tablet with M2 chip, Liquid Retina XDR display, and Apple Pencil support.", "iPad Pro 12.9\"", 1099.99m, 18 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Currency", "Description", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { 6, "USD", "Professional 4K monitor with USB-C connectivity and color accuracy certification.", "Dell UltraSharp 27\" 4K Monitor", 649.99m, 12 },
                    { 7, "USD", "Wireless mouse with ultra-fast scrolling and customizable buttons for productivity.", "Logitech MX Master 3S", 99.99m, 40 },
                    { 8, "USD", "RGB mechanical keyboard with Cherry MX switches and programmable macros.", "Mechanical Gaming Keyboard", 149.99m, 35 },
                    { 9, "USD", "360-degree Bluetooth speaker with deep bass and 16-hour battery life.", "Bose SoundLink Revolve+", 299.99m, 22 },
                    { 10, "USD", "Full-frame mirrorless camera with 24.2MP sensor and advanced autofocus system.", "Canon EOS R6 Mark II", 2499.99m, 8 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "High-performance laptop with 16GB RAM and 512GB SSD.", "Laptop", 999.99m, 10 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Latest model smartphone with a stunning display and powerful camera.", "Smartphone", 499.99m, 20 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Noise-cancelling headphones with superior sound quality.", "Headphones", 199.99m, 15 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Stylish smartwatch with fitness tracking and notifications.", "Smartwatch", 299.99m, 25 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Name", "Price", "Quantity" },
                values: new object[] { "Lightweight tablet with a vibrant display and long battery life.", "Tablet", 399.99m, 12 });
        }
    }
}
