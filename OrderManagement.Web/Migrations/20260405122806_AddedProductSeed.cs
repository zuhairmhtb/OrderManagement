using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderManagement.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedProductSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Currency", "Description", "Name", "Price", "Quantity" },
                values: new object[,]
                {
                    { 1, "USD", "High-performance laptop with 16GB RAM and 512GB SSD.", "Laptop", 999.99m, 10 },
                    { 2, "USD", "Latest model smartphone with a stunning display and powerful camera.", "Smartphone", 499.99m, 20 },
                    { 3, "USD", "Noise-cancelling headphones with superior sound quality.", "Headphones", 199.99m, 15 },
                    { 4, "USD", "Stylish smartwatch with fitness tracking and notifications.", "Smartwatch", 299.99m, 25 },
                    { 5, "USD", "Lightweight tablet with a vibrant display and long battery life.", "Tablet", 399.99m, 12 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
