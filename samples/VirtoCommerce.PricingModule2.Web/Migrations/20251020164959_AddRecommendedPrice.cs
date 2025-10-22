using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtoCommerce.PricingModule2.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Price",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "Price2Entity");

            migrationBuilder.AddColumn<decimal>(
                name: "RecommendedPrice",
                table: "Price",
                type: "Money",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Price");

            migrationBuilder.DropColumn(
                name: "RecommendedPrice",
                table: "Price");
        }
    }
}
