using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class productAddition2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 3, 2, 2, "Product 3 Description", 0.0, "", "Product 3", 200.0, "123450", 2, "M" },
                    { 4, 2, 2, "Product 4 Description", 0.0, "", "Product 4", 200.0, "123488", 2, "XL" }
                });

            migrationBuilder.InsertData(
                table: "SKU",
                columns: new[] { "SKUID", "Code" },
                values: new object[,]
                {
                    { 3, "NIK-WC-PRO-M" },
                    { 4, "NIK-WC-PRO-XL" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 4);
        }
    }
}
