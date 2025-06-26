using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskzen.Data.Migrations
{
    /// <inheritdoc />
    public partial class addedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_CreatedBy",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CreatedBy_Active",
                table: "Schedules",
                columns: new[] { "CreatedBy", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_EffectiveFrom_Active",
                table: "Schedules",
                columns: new[] { "EffectiveFrom", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_Date",
                table: "Leaves",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedBy_Date_Active",
                table: "Appointments",
                columns: new[] { "CreatedBy", "Date", "Active" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Date_Active",
                table: "Appointments",
                columns: new[] { "Date", "Active" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schedules_CreatedBy_Active",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_EffectiveFrom_Active",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_Date",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CreatedBy_Date_Active",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Date_Active",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_CreatedBy",
                table: "Schedules",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments",
                column: "CreatedBy");
        }
    }
}
