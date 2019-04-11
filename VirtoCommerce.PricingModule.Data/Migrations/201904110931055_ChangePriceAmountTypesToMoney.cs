namespace VirtoCommerce.PricingModule.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ChangePriceAmountTypesToMoney : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Price", "Sale", c => c.Decimal(storeType: "money"));
            AlterColumn("dbo.Price", "List", c => c.Decimal(nullable: false, storeType: "money"));
        }

        public override void Down()
        {
            AlterColumn("dbo.Price", "List", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Price", "Sale", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
