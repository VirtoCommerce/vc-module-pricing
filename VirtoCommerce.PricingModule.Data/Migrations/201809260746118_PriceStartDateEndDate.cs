namespace VirtoCommerce.PricingModule.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PriceStartDateEndDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Price", "StartDate", c => c.DateTime());
            AddColumn("dbo.Price", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Price", "EndDate");
            DropColumn("dbo.Price", "StartDate");
        }
    }
}
