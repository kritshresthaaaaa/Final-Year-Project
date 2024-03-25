using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeRelationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeRelationId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeRelationId",
                table: "AspNetUsers");
        }
    }
}
