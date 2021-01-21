using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.PricingModule.Data.Migrations
{
    public partial class AddProductPriceDatesIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Price_PricelistId",
                table: "Price");

            migrationBuilder.DropIndex(
                name: "IX_PriceId",
                table: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_PricelistProductDates",
                table: "Price",
                columns: new[] { "PricelistId", "ProductId", "StartDate", "EndDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PricelistProductDates",
                table: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Price_PricelistId",
                table: "Price",
                column: "PricelistId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceId",
                table: "Price",
                columns: new[] { "ProductId", "PricelistId" });
        }
    }
}
