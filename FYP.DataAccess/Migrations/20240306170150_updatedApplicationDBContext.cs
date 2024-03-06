using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updatedApplicationDBContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SKU",
                columns: new[] { "SKUID", "Code" },
                values: new object[,]
                {
                    { 1, "GUC-MC-BLA-S" },
                    { 2, "NIK-WC-FLO-M" },
                    { 3, "NIK-WC-PRO-M" },
                    { 4, "NIK-WC-PRO-XL" }
                });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 1, 1, 1, "Product 1 Description", 0.0, "", "Black Tshirt", 100.0, "123456", 1, "S" },
                    { 2, 2, 2, "Product 2 Description", 0.0, "", "Florence Tshirt", 200.0, "123457", 2, "M" },
                    { 3, 2, 2, "Product 3 Description", 0.0, "", "Product 3", 200.0, "123450", 2, "M" },
                    { 4, 2, 2, "Product 4 Description", 0.0, "", "Product 4", 200.0, "123488", 2, "XL" },
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
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 4);

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

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 2);
        }
    }
}
