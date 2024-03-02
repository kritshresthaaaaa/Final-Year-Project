using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class productAddition3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 5, 2, 2, "Product 5 Description", 0.0, "", "Product 5", 200.0, "123498", 2, "XL" },
                    { 6, 2, 2, "Product 6 Description", 0.0, "", "Product 6", 200.0, "123490", 2, "XL" },
                    { 7, 2, 2, "Product 7 Description", 0.0, "", "Product 7", 200.0, "123496", 2, "XL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
