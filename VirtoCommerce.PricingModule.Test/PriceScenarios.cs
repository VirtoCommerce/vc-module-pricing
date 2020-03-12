using System;
using System.Linq;
using Moq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Model;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{
    [Trait("Category", "CI")]
    public class PriceScenarios
    {
        [Fact]
        public void Get_document_changes_from_multiple_data_sources()
        {
            var mockOperationsLogs = new[]
            {
                new OperationLogEntity { ModifiedDate = new DateTime(2018, 11, 01), ObjectId = "1", ObjectType = nameof(PriceEntity) },
                new OperationLogEntity { ModifiedDate = new DateTime(2018, 11, 01), ObjectId = "2", ObjectType = nameof(PriceEntity) },
                new OperationLogEntity { ModifiedDate = new DateTime(2018, 11, 01), ObjectId = "3", ObjectType = nameof(PriceEntity) },
            };
            var mockPrices = new[] {
                //Without from/till dates (unbounded)
                new PriceEntity { Id = "1", ProductId = "1" },
                //Without from date (unbounded)
                new PriceEntity { Id = "2", ProductId = "2", EndDate = new DateTime(2018, 06, 01) },
                //Without till date (bounded)
                new PriceEntity { Id = "3", ProductId = "3", StartDate = new DateTime(2018, 06, 01) },
                //with from/till dates (bounded)
                new PriceEntity { Id = "4", ProductId = "4", StartDate = new DateTime(2018, 06, 01), EndDate = new DateTime(2018, 12, 01) }
            };
            var mockPricingRepository = new Mock<IPricingRepository>();
            mockPricingRepository.SetupGet(x => x.Prices).Returns(mockPrices.AsQueryable());
            mockPricingRepository.Setup(x => x.GetPricesByIds(It.IsAny<string[]>())).Returns((string[] ids) => ids.Select(x => new PriceEntity { ProductId = x }).ToArray());
            var mockPlatformRepository = new Mock<IPlatformRepository>();
            mockPlatformRepository.SetupGet(x => x.OperationLogs).Returns(mockOperationsLogs.AsQueryable());
            var mockSettingManager = new Mock<ISettingsManager>();
            mockSettingManager.Setup(x => x.GetValue(It.IsAny<string>(), It.IsAny<string>())).Returns(DateTime.MinValue.ToString());
            var changesProvider = new ProductPriceDocumentChangesProvider(() => mockPricingRepository.Object, () => mockPlatformRepository.Object, mockSettingManager.Object);

            var startDate = new DateTime(2018, 06, 01);
            var endDate = new DateTime(2018, 12, 01);
            var totalCount = changesProvider.GetTotalChangesCountAsync(startDate, endDate).GetAwaiter().GetResult();
            var changes = changesProvider.GetChangesAsync(startDate, endDate, 0, 5).GetAwaiter().GetResult();
            Assert.Equal(5, totalCount);
            Assert.Equal(new[] { "1", "2", "3", "3", "4" }, changes.Select(x => x.DocumentId));
            //Do not return calendar changes for not determined date interval
            changes = changesProvider.GetChangesAsync(startDate, null, 0, 5).GetAwaiter().GetResult();
            Assert.Equal(new[] { "1", "2", "3" }, changes.Select(x => x.DocumentId));

            //Paginated request for  multiple data sources (skip 2 from OperationLogEntity and take 3 from both data sources (OperationLogEntity, CalendarChanges))
            changes = changesProvider.GetChangesAsync(startDate, endDate, 2, 3).GetAwaiter().GetResult();
            Assert.Equal(new[] { "3", "3", "4" }, changes.Select(x => x.DocumentId));
        }

        [Fact]
        public void Can_return_prices_from_many_pricelists_by_priority()
        {
            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                PricelistIds = new[] { "Pricelist 1", "Pricelist 2", "Pricelist 3" }
            };

            var mockPrices = new[] {
                new Price { Id = "1", List = 10, MinQuantity = 2, PricelistId = "Pricelist 1", ProductId = "ProductId" },

                new Price {  Id = "2",List = 9, MinQuantity = 1, PricelistId = "Pricelist 2", ProductId = "ProductId" },
                new Price {  Id = "3",List = 10, MinQuantity = 2, PricelistId = "Pricelist 2", ProductId = "ProductId" },

                new Price {  Id = "4",List = 6, MinQuantity = 2, PricelistId = "Pricelist 3", ProductId = "ProductId" },
                new Price { Id = "5", List = 5, MinQuantity = 3,  PricelistId = "Pricelist 3", ProductId = "ProductId" }
            };

            var prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            // only 2 prices (from higher priority pricelists) returned, but not for MinQuantity == 3
            Assert.Equal(2, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[0].Id, prices[1].Id);
            Assert.DoesNotContain(prices, x => x.MinQuantity == 3);

            // Pricelist priority changed
            evalContext.PricelistIds = new[] { "Pricelist 3", "Pricelist 2", "Pricelist 1" };
            prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            // 3 prices returned, but not from "Pricelist 1"
            Assert.Equal(3, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[3].Id, prices[1].Id);
            Assert.Equal(mockPrices[4].Id, prices[2].Id);
            Assert.DoesNotContain(prices, x => x.PricelistId == "Pricelist 1");
        }

        [Fact]
        public void Can_return_price_from_many_prices_with_start_and_end_date()
        {
            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                PricelistIds = new[] { "List1" }
            };

            var mockPrices = new[] {
                // Unbounded past.
                new PriceEntity { Id = "1", List = 1, EndDate = new DateTime(2018, 09, 10), PricelistId = "List1" , ProductId = "ProductId" },
                // Bounded past.
                new PriceEntity { Id = "2", List = 2, StartDate = new DateTime(2018, 09, 15), EndDate = new DateTime(2018, 09, 17) , PricelistId = "List1" , ProductId = "ProductId" },

                // Bounded future.
                new PriceEntity { Id = "3", List = 3, StartDate = new DateTime(2018, 09, 26), EndDate = new DateTime(2018, 09, 29) , PricelistId = "List1" , ProductId = "ProductId" },
                // Unbounded future.
                new PriceEntity { Id = "4", List = 4, StartDate = new DateTime(2018, 10, 1), PricelistId = "List1" , ProductId = "ProductId" },

                // Default unfiltered price.
                new PriceEntity { Id = "10", List = 10, PricelistId = "List1" , ProductId = "ProductId" },
            };

            var mockRepository = new Mock<IPricingRepository>();
            mockRepository.SetupGet(x => x.Prices).Returns(mockPrices.AsQueryable());

            var service = new PricingServiceImpl(() => mockRepository.Object, null, null, null, null, null,
                new DefaultPricingPriorityFilterPolicy());

            // Eval with date and no matches, this should result in default price.
            evalContext.CertainDate = new DateTime(2018, 09, 20);
            var prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(10, prices.Single().List);

            // Eval with date falling in bounded future.
            evalContext.CertainDate = new DateTime(2018, 09, 27);
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(3, prices.Single().List);

            // Eval with date falling in unbounded future.
            evalContext.CertainDate = new DateTime(2118, 10, 2);
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(4, prices.Single().List);

            // Eval with date falling in bounded past.
            evalContext.CertainDate = new DateTime(2018, 9, 16);
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(2, prices.Single().List);

            // Eval with date falling in unbounded past.
            evalContext.CertainDate = new DateTime(2018, 8, 1);
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(1, prices.Single().List);

            // Eval with current date, should result in unbounded future price.
            evalContext.CertainDate = DateTime.UtcNow;
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(4, prices.Single().List);

            // Eval without date, should result in unbounded future price.
            // This is also a backwards compatibilty test.
            // CertainDate was not used in previous price evaluation. Default to 'now' behaviour.
            evalContext.CertainDate = null;
            prices = service.EvaluateProductPrices(evalContext);
            Assert.Equal(4, prices.Single().List);
        }
    }
}
