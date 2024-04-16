using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Produts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "ColorCode", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 1, 1, 1, "#FF5733", "Product 1 Description", 0.0, "", "Black Tshirt", 100.0, "123456", 1, "S" },
                    { 2, 2, 2, "#C70039", "Product 2 Description", 0.0, "", "Florence Tshirt", 200.0, "123457", 2, "M" },
                    { 3, 2, 2, "#C70039", "Product 3 Description", 0.0, "", "Product 3", 200.0, "123450", 2, "M" },
                    { 4, 2, 2, "#C70039", "Product 4 Description", 0.0, "", "Product 4", 200.0, "123488", 2, "XL" },
                    { 5, 2, 2, "#C70039", "Product 5 Description", 0.0, "", "Product 5", 200.0, "123498", 2, "XL" },
                    { 6, 2, 2, "#C70039", "Product 6 Description", 0.0, "", "Product 6", 200.0, "123490", 2, "XL" },
                    { 7, 2, 2, "#C70039", "Product 7 Description", 0.0, "", "Product 7", 200.0, "123496", 2, "XL" },
                    { 8, 1, 1, "#C70039", "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "12312312", 1, "S" },
                    { 9, 1, 1, "#C70039", "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "12312412", 1, "S" },
                    { 10, 1, 1, "#C70039", "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "1231241723", 5, "M" },
                    { 11, 1, 1, "#C70039", "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "1231999", 6, "L" }
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
                table: "Product",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
