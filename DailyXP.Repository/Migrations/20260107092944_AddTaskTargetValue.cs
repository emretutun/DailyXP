using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyXP.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskTargetValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetValue",
                table: "TaskDefinitions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "TaskDefinitions");
        }
    }
}
