using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskzen.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ModifiedBy",
                table: "Schedules",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_ModifiedBy",
                table: "Leaves",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ModifiedBy",
                table: "Appointments",
                column: "ModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_ModifiedBy",
                table: "Appointments",
                column: "ModifiedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Users_ModifiedBy",
                table: "Leaves",
                column: "ModifiedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_ModifiedBy",
                table: "Schedules",
                column: "ModifiedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_ModifiedBy",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Users_ModifiedBy",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_ModifiedBy",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_ModifiedBy",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_ModifiedBy",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ModifiedBy",
                table: "Appointments");
        }
    }
}
