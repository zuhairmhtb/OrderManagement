using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Vat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdditionalCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerContactNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ShippingStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShippingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingStreet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BillingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BillingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BillingState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchasedProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchasedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasedProducts_CustomerOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "CustomerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Currency", "Description", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, "USD", "High-performance laptop with M2 Pro chip, 16GB RAM and 512GB SSD.", "MacBook Pro 16\"", 2499.99m, 15 },
                    { 2, "USD", "Latest iPhone with A17 Pro chip, titanium design, and advanced camera system.", "iPhone 15 Pro", 999.99m, 25 },
                    { 3, "USD", "Industry-leading noise canceling headphones with 30-hour battery life.", "Sony WH-1000XM5", 399.99m, 20 },
                    { 4, "USD", "Advanced smartwatch with health monitoring and fitness tracking capabilities.", "Apple Watch Series 9", 399.99m, 30 },
                    { 5, "USD", "Powerful tablet with M2 chip, Liquid Retina XDR display, and Apple Pencil support.", "iPad Pro 12.9\"", 1099.99m, 18 },
                    { 6, "USD", "Professional 4K monitor with USB-C connectivity and color accuracy certification.", "Dell UltraSharp 27\" 4K Monitor", 649.99m, 12 },
                    { 7, "USD", "Wireless mouse with ultra-fast scrolling and customizable buttons for productivity.", "Logitech MX Master 3S", 99.99m, 40 },
                    { 8, "USD", "RGB mechanical keyboard with Cherry MX switches and programmable macros.", "Mechanical Gaming Keyboard", 149.99m, 35 },
                    { 9, "USD", "360-degree Bluetooth speaker with deep bass and 16-hour battery life.", "Bose SoundLink Revolve+", 299.99m, 22 },
                    { 10, "USD", "Full-frame mirrorless camera with 24.2MP sensor and advanced autofocus system.", "Canon EOS R6 Mark II", 2499.99m, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_City",
                table: "Address",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Country",
                table: "Address",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Address_CustomerId",
                table: "Address",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_PostalCode",
                table: "Address",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerContactNumber",
                table: "CustomerOrders",
                column: "CustomerContactNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerEmail",
                table: "CustomerOrders",
                column: "CustomerEmail");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerId",
                table: "CustomerOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_OrderDate",
                table: "CustomerOrders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_OrderStatus",
                table: "CustomerOrders",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_TotalAmount",
                table: "CustomerOrders",
                column: "TotalAmount");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_Name",
                table: "PurchasedProducts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_OrderId",
                table: "PurchasedProducts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_Price",
                table: "PurchasedProducts",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasedProducts_ProductId",
                table: "PurchasedProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PurchasedProducts");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "CustomerOrders");
        }
    }
}
