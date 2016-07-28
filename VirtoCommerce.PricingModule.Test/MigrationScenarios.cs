using System.Linq;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Testing.Bases;
using VirtoCommerce.PricingModule.Data.Migrations;
using VirtoCommerce.PricingModule.Data.Repositories;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{
    public class MigrationScenarios : MigrationsTestBase
    {
        [Fact]
        [Trait("Category", "CI")]
        public void Can_create_pricing_new_database()
        {
            DropDatabase();

            var migrator = CreateMigrator<Configuration>();

            using (var context = CreateContext<PricingRepositoryImpl>())
            {
                context.Database.CreateIfNotExists();
                new SetupDatabaseInitializer<PricingRepositoryImpl, Configuration>().InitializeDatabase(context);
                Assert.Equal(0, context.Pricelists.Count());
            }

            // remove all migrations
            migrator.Update("0");
            Assert.False(TableExists("Pricelist"));
            var existTables = Info.Tables.Any();
            Assert.False(existTables);

            DropDatabase();
        }
    }
}
