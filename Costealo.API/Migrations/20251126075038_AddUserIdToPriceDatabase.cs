using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Costealo.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPriceDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "PriceDatabases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PriceDatabases_UserId",
                table: "PriceDatabases",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceDatabases_Users_UserId",
                table: "PriceDatabases",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceDatabases_Users_UserId",
                table: "PriceDatabases");

            migrationBuilder.DropIndex(
                name: "IX_PriceDatabases_UserId",
                table: "PriceDatabases");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PriceDatabases");
        }
    }
}
