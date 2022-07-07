using System;
using System.Linq;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Model;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.PricingModule.Data.Services;
using VirtoCommerce.PricingModule.Data.Validators;
using Xunit;

#pragma warning disable CS0618 // Allow to use obsoleted

namespace VirtoCommerce.PricingModule.Test
{
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

            var mockPrices = new PriceEntity[] {
                //Without from/till dates (unbounded)
                new PriceEntity { Id = "1", ProductId = "1" },
                //Without from date (unbounded)
                new PriceEntity { Id = "2", ProductId = "2", EndDate = new DateTime(2018, 06, 01) },
                //Without till date (bounded)
                new PriceEntity { Id = "3", ProductId = "3", StartDate = new DateTime(2018, 06, 01) },
                //with from/till dates (bounded)
                new PriceEntity { Id = "4", ProductId = "4", StartDate = new DateTime(2018, 06, 01), EndDate = new DateTime(2018, 12, 01) }
            }.AsQueryable().BuildMock();

            var mockPricingRepository = new Mock<IPricingRepository>();
            mockPricingRepository.SetupGet(x => x.Prices).Returns(mockPrices.Object);
            mockPricingRepository.Setup(x => x.GetPricesByIdsAsync(It.IsAny<string[]>())).ReturnsAsync(mockPrices.Object.ToArray());

            var mockPlatformRepository = new Mock<IPlatformRepository>();
            mockPlatformRepository.SetupGet(x => x.OperationLogs).Returns(mockOperationsLogs.AsQueryable());

            var mockSettingManager = new Mock<ISettingsManager>();
            mockSettingManager.Setup(s => s.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry() { Value = DateTime.MinValue });

            var mockChangeLogSearchService = new Mock<IChangeLogSearchService>();
            mockChangeLogSearchService.Setup(x => x.SearchAsync(It.IsAny<ChangeLogSearchCriteria>()))
                .ReturnsAsync(new ChangeLogSearchResult
                {
                    Results = mockOperationsLogs.Select(o => o.ToModel(AbstractTypeFactory<OperationLog>.TryCreateInstance())).ToList(),
                    TotalCount = mockOperationsLogs.Select(o => o.ToModel(AbstractTypeFactory<OperationLog>.TryCreateInstance())).Count()
                });

            var changesProvider = new ProductPriceDocumentChangesProvider(mockChangeLogSearchService.Object, mockSettingManager.Object, () => mockPricingRepository.Object);

            var startDate = new DateTime(2018, 06, 01);
            var endDate = new DateTime(2018, 12, 01);

            var totalCount = changesProvider.GetTotalChangesCountAsync(startDate, endDate).GetAwaiter().GetResult();
            var changes = changesProvider.GetChangesAsync(startDate, endDate, 0, 5).GetAwaiter().GetResult();

            Assert.Equal(5, totalCount);
            Assert.Equal(new[] { "1", "2", "3", "3", "4" }, changes.Select(x => x.DocumentId));

            //Do not return calendar changes for not determined date interval
            changes = changesProvider.GetChangesAsync(startDate, null, 0, 5).GetAwaiter().GetResult();
            Assert.Equal(new[] { "1", "2", "3" }, changes.Select(x => x.DocumentId));

            mockSettingManager.Setup(s => s.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry() { Value = DateTime.MinValue });

            //Paginated request for  multiple data sources (skip 2 from OperationLogEntity and take 3 from both data sources (OperationLogEntity, CalendarChanges))
            changes = changesProvider.GetChangesAsync(startDate, endDate, 2, 3).GetAwaiter().GetResult();
            Assert.Equal(new[] { "3", "3", "4" }, changes.Select(x => x.DocumentId));
        }


        [Fact]
        public void Can_return_prices_from_many_pricelists_by_priority()
        {
            var pricelists = new Pricelist[]
            {
                new() { Id = "Pricelist 1", Priority = 3 },
                new() { Id = "Pricelist 2", Priority = 2 },
                new() { Id = "Pricelist 3", Priority = 1 }
            };

            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                Pricelists = pricelists
            };

            var mockPrices = new[] {
                new Price { Id = "1", List = 10, MinQuantity = 2, PricelistId = "Pricelist 1", ProductId = "ProductId" },

                new Price { Id = "2", List = 9, MinQuantity = 1, PricelistId = "Pricelist 2", ProductId = "ProductId" },
                new Price { Id = "3", List = 10, MinQuantity = 2, PricelistId = "Pricelist 2", ProductId = "ProductId" },

                new Price { Id = "4", List = 6, MinQuantity = 2, PricelistId = "Pricelist 3", ProductId = "ProductId" },
                new Price { Id = "5", List = 5, MinQuantity = 3, PricelistId = "Pricelist 3", ProductId = "ProductId" }
            };

            var prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            // only 2 prices (from higher priority pricelists) returned, but not for MinQuantity == 3
            Assert.Equal(2, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[0].Id, prices[1].Id);
            Assert.DoesNotContain(prices, x => x.MinQuantity == 3);

            // Pricelist priority changed
            evalContext.Pricelists[0].Priority = 1;
            evalContext.Pricelists[1].Priority = 2;
            evalContext.Pricelists[2].Priority = 3;

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
            var pricelist = new Pricelist { Id = "List1", Priority = 0, };

            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                Pricelists = new [] { pricelist }
            };

            var mockPrices = new PriceEntity[] {
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
            }.AsQueryable().BuildMock();

            var mockRepository = new Mock<IPricingRepository>();
            mockRepository.SetupGet(x => x.Prices).Returns(mockPrices.Object);

            var service = new PricingServiceImpl(new PricelistAssignmentService(() => mockRepository.Object, null, null, new PricelistAssignmentsValidator()),
                        new PricelistService(() => mockRepository.Object, null, null),
                        new PriceService(() => mockRepository.Object, null, null, null),
                        new PricingEvaluatorService(
                        () => mockRepository.Object, null, null, null,
                        new DefaultPricingPriorityFilterPolicy()));

            // Eval with date and no matches, this should result in default price.
            evalContext.CertainDate = new DateTime(2018, 09, 20);
            var prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(10, prices.Single().List);

            // Eval with date falling in bounded future.
            evalContext.CertainDate = new DateTime(2018, 09, 27);
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(3, prices.Single().List);

            // Eval with date falling in unbounded future.
            evalContext.CertainDate = new DateTime(2118, 10, 2);
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(4, prices.Single().List);

            // Eval with date falling in bounded past.
            evalContext.CertainDate = new DateTime(2018, 9, 16);
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(2, prices.Single().List);

            // Eval with date falling in unbounded past.
            evalContext.CertainDate = new DateTime(2018, 8, 1);
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(1, prices.Single().List);

            // Eval with current date, should result in unbounded future price.
            evalContext.CertainDate = DateTime.UtcNow;
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(4, prices.Single().List);

            // Eval without date, should result in unbounded future price.
            // This is also a backwards compatibilty test.
            // CertainDate was not used in previous price evaluation. Default to 'now' behaviour.
            evalContext.CertainDate = null;
            prices = service.EvaluateProductPricesAsync(evalContext).GetAwaiter().GetResult();
            Assert.Equal(4, prices.Single().List);
        }

        [Fact]
        public void Can_return_prices_from_many_pricelists_by_price()
        {
            var pricelists = new Pricelist[]
            {
                new() { Id = "Pricelist 1", Priority = 1 },
                new() { Id = "Pricelist 2", Priority = 1 },
                new() { Id = "Pricelist 3", Priority = 1 }
            };

            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                Pricelists = pricelists
            };

            var mockPrices = new[] {
                new Price { Id = "1", List = 4, MinQuantity = 1, PricelistId = "Pricelist 1", ProductId = "ProductId" },
                new Price { Id = "2", List = 5, MinQuantity = 1, PricelistId = "Pricelist 2", ProductId = "ProductId" },
                new Price { Id = "3", List = 6, MinQuantity = 1, PricelistId = "Pricelist 2", ProductId = "ProductId" },
            };

            var prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            Assert.Single(prices);
            Assert.Equal(mockPrices[0].Id, prices.First().Id);

            mockPrices[1].List = 3;

            prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            Assert.Single(prices);
            Assert.Equal(mockPrices[1].Id, prices.First().Id);
        }

        //[Fact]
        //public async Task Can_return_pri()
        //{
        //    RegisterTypes();
        //    var evalContext = new PriceEvaluationContext
        //    {
        //        ProductIds = new[] { "ProductId" },
        //        PricelistIds = new[] { "Pricelist 1", "Pricelist 2", "Pricelist 3" }
        //    };

        //    var mockPrices = new Common.TestAsyncEnumerable<PriceEntity>(new List<PriceEntity> {
        //        new PriceEntity { List = 10, MinQuantity = 2, PricelistId = "Pricelist 1", Id = "1", ProductId = "ProductId" },

        //        new PriceEntity { List = 9, MinQuantity = 1, PricelistId = "Pricelist 2", Id = "2", ProductId = "ProductId" },
        //        new PriceEntity { List = 10, MinQuantity = 2, PricelistId = "Pricelist 2", Id = "3", ProductId = "ProductId" },

        //        new PriceEntity { List = 6, MinQuantity = 2, PricelistId = "Pricelist 3", Id = "4", ProductId = "ProductId" },
        //        new PriceEntity { List = 5, MinQuantity = 3,  PricelistId = "Pricelist 3", Id = "5", ProductId = "ProductId" }
        //    });

        //    var pricingService = GetPricingService(() => GetPricingRepositoryMock(mockPrices));
        //    //var pricesMock = GetMockPrices().Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance()))
        //    //    .Where(p => evalContext.ProductIds.Contains(p.ProductId))
        //    //    .Where(p => evalContext.PricelistIds.Contains(p.PricelistId))
        //    //    .Where(p => p.MinQuantity < evalContext.Quantity);

        //    //_pricingPriorityFilterPolicy.Setup(x => x.FilterPrices(It.IsAny<IEnumerable<Price>>(), evalContext))
        //    //    .Returns(pricesMock);

        //    //Act
        //    var prices = (await pricingService.EvaluateProductPricesAsync(evalContext)).ToArray();

        //    // only 2 prices (from higher priority pricelists) returned, but not for MinQuantity == 3
        //    Assert.Equal(2, prices.Length);
        //    //Assert.Equal(mockPrices[1].Id, prices[0].Id);
        //    //Assert.Equal(mockPrices[0].Id, prices[1].Id);
        //    Assert.DoesNotContain(prices, x => x.MinQuantity == 3);

        //    // Pricelist priority changed
        //    evalContext.PricelistIds = new[] { "Pricelist 3", "Pricelist 2", "Pricelist 1" };
        //    prices = (await pricingService.EvaluateProductPricesAsync(evalContext)).ToArray();

        //    // 3 prices returned, but not from "Pricelist 1"
        //    Assert.Equal(3, prices.Length);
        //    //Assert.Equal(mockPrices[1].Id, prices[0].Id);
        //    //Assert.Equal(mockPrices[3].Id, prices[1].Id);
        //    //Assert.Equal(mockPrices[4].Id, prices[2].Id);
        //    Assert.DoesNotContain(prices, x => x.PricelistId == "Pricelist 1");
        //}

        //private IPricingService GetPricingService(Func<IPricingRepository> repositoryFactory)
        //{
        //    var logger = new Moq.Mock<ILogger<PricingServiceImpl>>();
        //    var platformMemoryCache = new Mock<IPlatformMemoryCache>();
        //    var cacheEntry = new Mock<ICacheEntry>();
        //    cacheEntry.SetupGet(c => c.ExpirationTokens).Returns(new List<IChangeToken>());
        //    var cacheKey = CacheKey.With(typeof(PricingServiceImpl), "EvaluatePriceListsAsync");
        //    platformMemoryCache.Setup(pmc => pmc.CreateEntry(cacheKey)).Returns(cacheEntry.Object);
        //    return new PricingServiceImpl(repositoryFactory, null, logger.Object, platformMemoryCache.Object,  null, _pricingPriorityFilterPolicy.Object);
        //}

        //private IPricingRepository GetPricingRepository()
        //{
        //    var dbContextFactory = new DesignTimeDbContextFactory();
        //    var dbContext = dbContextFactory.CreateDbContext(new string[0]);

        //    var result = new PricingRepositoryImpl(dbContext);
        //    return result;
        //}

        //private IPricingRepository GetPricingRepositoryMock(IEnumerable<PriceEntity> prices)
        //{
        //    var mock = new Moq.Mock<IPricingRepository>();
        //    mock.Setup(foo => foo.Prices).Returns(prices.AsQueryable());

        //    return mock.Object;
        //}

        //private void RegisterTypes()
        //{
        //    if (AbstractTypeFactory<IConditionTree>.AllTypeInfos.All(t => t.Type != typeof(PriceConditionTree)))
        //        AbstractTypeFactory<IConditionTree>.RegisterType<PriceConditionTree>();

        //    if (AbstractTypeFactory<IConditionTree>.AllTypeInfos.All(t => t.Type != typeof(BlockPricingCondition)))
        //        AbstractTypeFactory<IConditionTree>.RegisterType<BlockPricingCondition>();

        //    if (AbstractTypeFactory<IConditionTree>.AllTypeInfos.All(t => t.Type != typeof(UserGroupsContainsCondition)))
        //        AbstractTypeFactory<IConditionTree>.RegisterType<UserGroupsContainsCondition>();


        //}
    }


}
