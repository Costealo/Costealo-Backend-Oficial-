using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Costealo.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkbooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workbooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductionUnits = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ProfitMarginPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TargetSalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OperationalCostPercentage = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OperationalCostFixed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workbooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workbooks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkbookItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkbookId = table.Column<int>(type: "int", nullable: false),
                    PriceItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdditionalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkbookItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkbookItems_PriceItems_PriceItemId",
                        column: x => x.PriceItemId,
                        principalTable: "PriceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkbookItems_Workbooks_WorkbookId",
                        column: x => x.WorkbookId,
                        principalTable: "Workbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkbookItems_PriceItemId",
                table: "WorkbookItems",
                column: "PriceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkbookItems_WorkbookId",
                table: "WorkbookItems",
                column: "WorkbookId");

            migrationBuilder.CreateIndex(
                name: "IX_Workbooks_UserId",
                table: "Workbooks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkbookItems");

            migrationBuilder.DropTable(
                name: "Workbooks");
        }
    }
}
