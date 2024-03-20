using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fyp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class changedInNotificationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToRoomId",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "ToRoomName",
                table: "Notification",
                newName: "ToEmployeeName");

            migrationBuilder.AddColumn<Guid>(
                name: "ToEmployeeId",
                table: "Notification",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToEmployeeId",
                table: "Notification");

            migrationBuilder.RenameColumn(
                name: "ToEmployeeName",
                table: "Notification",
                newName: "ToRoomName");

            migrationBuilder.AddColumn<int>(
                name: "ToRoomId",
                table: "Notification",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
