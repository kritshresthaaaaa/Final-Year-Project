using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "BrandID", "CategoryID", "Description", "DiscountedPrice", "ImageUrl", "Name", "Price", "RFIDTag", "SKUID", "Sizes" },
                values: new object[,]
                {
                    { 10, 1, 1, "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "1231241723", 5, "M" },
                    { 11, 1, 1, "black t prodyc 1 ", 0.0, "", "Black Tshirt", 100.0, "1231999", 6, "L" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
