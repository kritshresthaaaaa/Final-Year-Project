using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class skuAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SKU",
                columns: new[] { "SKUID", "Code" },
                values: new object[,]
                {
                    { 5, "GUC-MC-BLA-M" },
                    { 6, "GUC-MC-BLA-L" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "SKU",
                keyColumn: "SKUID",
                keyValue: 6);
        }
    }
}
