using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class productAddition : Migration
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
                    { 2, "NIK-WC-FLO-M" }
                });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 1, 1, 1, "Product 1 Description", 0.0, "", "Black Tshirt", 100.0, "123456", 1, "S" },
                    { 2, 2, 2, "Product 2 Description", 0.0, "", "Florence Tshirt", 200.0, "123457", 2, "M" }
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
