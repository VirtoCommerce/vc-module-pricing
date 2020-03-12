namespace VirtoCommerce.PricingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewPriceIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Price", new[] { "ProductId", "PricelistId" }, name: "ProductIdAndPricelistId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Price", "ProductIdAndPricelistId");
        }
    }
}
