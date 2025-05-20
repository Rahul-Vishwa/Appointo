using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskzen.Data.Migrations
{
    /// <inheritdoc />
    public partial class Schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Schedules",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Schedules",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "BreakStartDate",
                table: "Schedules",
                newName: "BreakStartTime");

            migrationBuilder.RenameColumn(
                name: "BreakEndDate",
                table: "Schedules",
                newName: "BreakEndTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Schedules",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Schedules",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "BreakStartTime",
                table: "Schedules",
                newName: "BreakStartDate");

            migrationBuilder.RenameColumn(
                name: "BreakEndTime",
                table: "Schedules",
                newName: "BreakEndDate");
        }
    }
}
