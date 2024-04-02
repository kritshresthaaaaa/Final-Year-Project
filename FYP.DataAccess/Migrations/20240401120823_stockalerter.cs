using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class stockalerter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockAlerter",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockAlerter",
                table: "AspNetUsers");
        }
    }
}
