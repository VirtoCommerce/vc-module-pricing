using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    // Shares a collection with PriceEvaluationCacheTests — see the note there. ExpireRegion_DropsEntry
    // below fires a process-wide GenericCachingRegion<Price>.ExpireRegion(); without this, xUnit's
    // default cross-class parallelism lets it evict PriceEvaluationCacheTests' warm entries mid-run.
    [Collection(nameof(PriceEvaluationCacheCollection))]
    public class PriceEvaluationInvalidationTests
    {
        // Exposes the protected PriceService.ClearCache override so tests can drive the real
        // per-key invalidation path directly, without a full SaveChangesAsync/DeleteAsync round-trip.
        private sealed class TestablePriceService : PriceService
        {
            public TestablePriceService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
                IEventPublisher eventPublisher, IPricelistService pricelistService)
                : base(repositoryFactory, platformMemoryCache, eventPublisher, pricelistService)
            {
            }

            public new void ClearCache(IList<Price> models) => base.ClearCache(models);
        }

        private static TestablePriceService BuildPriceService(IPlatformMemoryCache cache) =>
            new(() => new Mock<IPricingRepository>().Object, cache, new Mock<IEventPublisher>().Object, new Mock<IPricelistService>().Object);

        // Editing product X must not evict product Y's evaluator-cache entry.
        [Fact]
        public async Task ClearCache_EditsProductX_InvalidatesOnlyX()
        {
            var (service, _, batchCount, testable) = PriceEvaluationCacheTests.BuildService(new[]
            {
                PriceEvaluationCacheTests.SinglePrice("prodX").Single(),
                PriceEvaluationCacheTests.SinglePrice("prodY").Single(),
            });

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prodX", "prodY"));
            Assert.Equal(1, batchCount());

            GenericCachingRegion<Price>.ExpireTokenForKey(PriceEvaluationCacheKey.TokenKey("List1", "prodX"));

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prodX", "prodY"));

            Assert.Equal(2, batchCount()); // exactly one reload fired
            Assert.Contains("prodX", testable.LoadBatches[1]);
            Assert.DoesNotContain("prodY", testable.LoadBatches[1]); // Y stayed warm
        }

        // A region-wide flush still drops the entry — proves the per-key token is actually
        // composited into the entry's change token, not orphaned.
        [Fact]
        public async Task ExpireRegion_DropsEntry()
        {
            var (service, _, batchCount, _) = PriceEvaluationCacheTests.BuildService(PriceEvaluationCacheTests.SinglePrice("prod1"));

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));
            Assert.Equal(1, batchCount());

            GenericCachingRegion<Price>.ExpireRegion();

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));

            Assert.Equal(2, batchCount());
        }

        // A real PriceService.ClearCache call, sharing the evaluator's cache,
        // must make the next evaluation observe the updated price.
        [Fact]
        public async Task ClearCache_RealPriceServiceUpdate_NextEvalReflectsChange()
        {
            var cache = PriceEvaluationCacheTests.CreateCache();
            var dbPrices = new List<PriceEntity>(PriceEvaluationCacheTests.SinglePrice("prod1"));

            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(() => dbPrices.BuildMock());
            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });
            var evaluator = new PricingEvaluatorService(() => mock.Object, null, null, cache, new DefaultPricingPriorityFilterPolicy(), settings.Object);
            var priceService = BuildPriceService(cache);

            var first = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));
            Assert.Equal(10, first.Single().List);

            dbPrices[0].List = 20; // simulate the DB row already having been updated

            priceService.ClearCache(new List<Price> { new() { PricelistId = "List1", ProductId = "prod1" } });

            var second = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));

            Assert.Equal(20, second.Single().List);
        }

        // A real PriceService.ClearCache call after the price row is gone must make
        // the next evaluation stop returning it.
        [Fact]
        public async Task ClearCache_RealPriceServiceDelete_NextEvalNoLongerReturnsPrice()
        {
            var cache = PriceEvaluationCacheTests.CreateCache();
            var dbPrices = new List<PriceEntity>(PriceEvaluationCacheTests.SinglePrice("prod1"));

            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(() => dbPrices.BuildMock());
            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });
            var evaluator = new PricingEvaluatorService(() => mock.Object, null, null, cache, new DefaultPricingPriorityFilterPolicy(), settings.Object);
            var priceService = BuildPriceService(cache);

            var first = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));
            Assert.Single(first);

            dbPrices.Clear(); // simulate the DB row already having been deleted

            priceService.ClearCache(new List<Price> { new() { PricelistId = "List1", ProductId = "prod1" } });

            var second = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("prod1"));

            Assert.Empty(second);
        }

        // A price edit that moves a row to a different ProductId must
        // invalidate BOTH the old and the new (pricelistId, productId) evaluator-cache entries. The
        // old key is only observable from SaveChangesAsync (it sees the pre-edit ProductId via
        // changedEntries[i].OldEntry); ClearCache alone only ever sees the post-edit models.
        [Fact]
        public async Task ClearCache_MovesProduct_InvalidatesOldAndNewKey()
        {
            var cache = PriceEvaluationCacheTests.CreateCache();
            const string priceId = "price1";
            var dbPrices = new List<PriceEntity>
            {
                new() { Id = priceId, List = 10, PricelistId = "List1", ProductId = "X" },
            };

            var repositoryMock = new Mock<IPricingRepository>();
            repositoryMock.Setup(x => x.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);
            repositoryMock.Setup(x => x.GetPricesByIdsAsync(new[] { priceId }))
                .ReturnsAsync(() => new List<PriceEntity>(dbPrices));
            repositoryMock.SetupGet(x => x.Prices).Returns(() => dbPrices.BuildMock());

            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });

            var evaluator = new PricingEvaluatorService(() => repositoryMock.Object, null, null, cache, new DefaultPricingPriorityFilterPolicy(), settings.Object);
            var priceService = new PriceService(() => repositoryMock.Object, cache, new Mock<IEventPublisher>().Object, new Mock<IPricelistService>().Object);

            var firstX = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("X"));
            Assert.Equal(10, firstX.Single().List); // warm (List1, X)

            var movedPrice = new Price { Id = priceId, List = 15, PricelistId = "List1", ProductId = "Z" };
            await priceService.SaveChangesAsync(new[] { movedPrice }); // moves the row X -> Z

            var secondX = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("X"));
            var firstZ = await evaluator.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("Z"));

            Assert.Empty(secondX); // old key expired: reloaded and the row is gone from X
            Assert.Equal(15, firstZ.Single().List); // new key reflects the moved price
        }
    }
}
