using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MockQueryable;
using Moq;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    // Shares a collection with PriceEvaluationCacheTests / PriceEvaluationInvalidationTests — see the
    // note on PriceEvaluationCacheTests. These tests drive the private PriceEvaluationCache directly,
    // not the shared GenericCachingRegion<Price>, but keep the serialization for consistency with the
    // rest of the evaluator-cache suite.
    [Collection(nameof(PriceEvaluationCacheCollection))]
    public class PriceEvaluationBoundingTests
    {
        private static PriceEntity[] SingleRowEach(params string[] productIds) =>
            productIds
                .Select(id => new PriceEntity { Id = id + "-p", List = 10, PricelistId = "List1", ProductId = id })
                .ToArray();

        private static (PricingEvaluatorService service, PriceEvaluationCacheTests.TestablePricingEvaluatorService testable, PriceEvaluationCache cache)
            BuildBoundedService(int rowLimit, PriceEntity[] prices)
        {
            var mockPrices = prices.BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);

            var settings = PriceEvaluationCacheTests.CreateSettingsMock(rowLimit: rowLimit);
            var cache = new PriceEvaluationCache(settings.Object);
            var svc = new PriceEvaluationCacheTests.TestablePricingEvaluatorService(() => mock.Object, PriceEvaluationCacheTests.CreateCache(), settings.Object, cache);

            return (svc, svc, cache);
        }

        // AC-13: driving the private cache past RowLimit must shed entries (graceful degradation),
        // never throw or grow unbounded. Not asserting a specific victim or exact residents.
        [Fact]
        public async Task Bounding_ExceedsRowLimit_EvictsAndRefetches()
        {
            var (service, testable, cache) = BuildBoundedService(2, SingleRowEach("p1", "p2", "p3"));

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1"));
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p2"));
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p3"));

            // [I4] MemoryCache overcapacity compaction runs on a background ThreadPool thread, not
            // synchronously with Set — force it deterministically so eviction has actually happened
            // before we assert on it.
            ((MemoryCache)cache.Cache).Compact(0.5);

            var before = testable.LoadBatches.Count;

            // p1 is expected to be among the evicted entries after compaction — assert it reloads.
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1"));

            Assert.True(testable.LoadBatches.Count > before);
        }

        // [C1] A non-positive/invalid/unset RowLimit setting must fall back to the documented default
        // (100000), never to Max(1, 0) == 1 — a degenerate ceiling would evict almost everything.
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void PriceEvaluationCache_NonPositiveRowLimit_FallsBackToDefault(int configuredRowLimit)
        {
            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(
                    ModuleConstants.Settings.General.PriceEvaluationCacheRowLimit.Name,
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = configuredRowLimit });

            using var cache = new PriceEvaluationCache(settings.Object);

            Assert.Equal(100000, cache.RowLimit);
        }

        [Fact]
        public void PriceEvaluationCache_NullSettingsManager_FallsBackToDefault()
        {
            using var cache = new PriceEvaluationCache(null);

            Assert.Equal(100000, cache.RowLimit);
        }
    }
}
