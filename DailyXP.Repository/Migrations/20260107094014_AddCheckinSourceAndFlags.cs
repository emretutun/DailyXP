using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyXP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckinSourceAndFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClientRecordedAtUtc",
                table: "UserDailyTaskLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspicious",
                table: "UserDailyTaskLogs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "UserDailyTaskLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientRecordedAtUtc",
                table: "UserDailyTaskLogs");

            migrationBuilder.DropColumn(
                name: "IsSuspicious",
                table: "UserDailyTaskLogs");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "UserDailyTaskLogs");
        }
    }
}
