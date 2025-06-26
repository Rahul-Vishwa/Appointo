using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskzen.Data.Migrations
{
    /// <inheritdoc />
    public partial class givenNameUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GivenName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GivenName",
                table: "Users");
        }
    }
}
