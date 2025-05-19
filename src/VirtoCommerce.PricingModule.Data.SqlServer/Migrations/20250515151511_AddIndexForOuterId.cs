using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PricingModule.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForOuterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PricelistAssignment_OuterId",
                table: "PricelistAssignment",
                column: "OuterId");

            migrationBuilder.CreateIndex(
                name: "IX_Price_OuterId",
                table: "Price",
                column: "OuterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PricelistAssignment_OuterId",
                table: "PricelistAssignment");

            migrationBuilder.DropIndex(
                name: "IX_Price_OuterId",
                table: "Price");
        }
    }
}
