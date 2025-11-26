using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Costealo.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Workbooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PriceDatabases",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Workbooks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PriceDatabases");
        }
    }
}
