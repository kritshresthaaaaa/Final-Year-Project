using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removedMinimumPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumPurchase",
                table: "Discount");

            migrationBuilder.AddColumn<double>(
                name: "OriginalPrice",
                table: "Product",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 1,
                column: "OriginalPrice",
                value: 0.0);

            migrationBuilder.UpdateData(
                table: "Product",
                keyColumn: "Id",
                keyValue: 2,
                column: "OriginalPrice",
                value: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Product");

            migrationBuilder.AddColumn<double>(
                name: "MinimumPurchase",
                table: "Discount",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
