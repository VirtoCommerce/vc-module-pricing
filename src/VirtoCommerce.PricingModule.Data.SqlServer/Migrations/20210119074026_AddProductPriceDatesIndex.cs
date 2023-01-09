using Microsoft.EntityFrameworkCore.Migrations;

namespace VirtoCommerce.PricingModule.Data.SqlServer.Migrations
{
    public partial class AddProductPriceDatesIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_Price_PricelistId] ON [Price]");

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_PriceId] ON [Price]");

            // Drop redundant index from 2X
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [ProductIdAndPricelistId] ON [Price]");

            // Drop redundant index from 2X
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS [IX_PricelistId] ON [Price]");


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
