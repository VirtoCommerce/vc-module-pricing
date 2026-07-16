using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    // Shares a collection with PriceEvaluationInvalidationTests: both exercise the static
    // GenericCachingRegion<Price> region, so xUnit must not run them concurrently with each other
    // (a region-wide ExpireRegion() in one would evict the other's warm entries mid-assertion).
    [Collection(nameof(PriceEvaluationCacheCollection))]
    public class PriceEvaluationCacheTests
    {
        // 3-arg ctor confirmed against PricingEvaluatorServiceTests.cs:185.
        internal static IPlatformMemoryCache CreateCache() =>
            new PlatformMemoryCache(new MemoryCache(new MemoryCacheOptions()),
                Options.Create(new CachingOptions()),
                new Mock<Microsoft.Extensions.Logging.ILogger<PlatformMemoryCache>>().Object);

        // Records EXACTLY which product ids reach the DB — the only sound "read volume" metric.
        // internal (not private): PriceEvaluationInvalidationTests shares BuildService, which returns
        // this type — a private nested type would make that method's signature inaccessible cross-class.
        internal sealed class TestablePricingEvaluatorService : PricingEvaluatorService
        {
            public readonly List<string[]> LoadBatches = new();
            public TestablePricingEvaluatorService(Func<IPricingRepository> f, IPlatformMemoryCache c, ISettingsManager s)
                : base(f, null, null, c, new DefaultPricingPriorityFilterPolicy(), s) { }

            protected override Task<IList<Price>> LoadPricesFromDatabaseAsync(IList<string> productIds, IList<string> pricelistIds)
            {
                LoadBatches.Add(productIds.ToArray());
                return base.LoadPricesFromDatabaseAsync(productIds, pricelistIds);
            }
        }

        internal static PriceEntity[] SinglePrice(string productId) =>
            new[] { new PriceEntity { Id = productId + "-p", List = 10, PricelistId = "List1", ProductId = productId } };

        internal static PriceEvaluationContext Context(params string[] productIds) => new()
        {
            ProductIds = productIds,
            Pricelists = new[] { new Pricelist { Id = "List1", Priority = 0 } },
        };

        // dbCalls = cumulative DISTINCT products actually loaded from DB (NOT factory invocations).
        // internal (not private): shared with PriceEvaluationInvalidationTests so the AC-12 precision
        // test can inspect testable.LoadBatches without duplicating this setup.
        internal static (PricingEvaluatorService service, Func<int> distinctLoaded, Func<int> batchCount, TestablePricingEvaluatorService testable) BuildService(PriceEntity[] prices)
        {
            var mockPrices = prices.BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);
            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });
            var svc = new TestablePricingEvaluatorService(() => mock.Object, CreateCache(), settings.Object);
            return (svc,
                () => svc.LoadBatches.SelectMany(x => x).Distinct().Count(),
                () => svc.LoadBatches.Count,
                svc);
        }

        // Load-recorder infra proof — the metric that Task 3b's warm-reuse assertions will build on.
        // Cache routing lands in Task 3b; today a second eval still re-loads, so this test only
        // proves the recorder itself captures the requested product on a single (non-cached) eval.
        [Fact]
        public async Task EvaluateProductPricesAsync_LoadRecorder_CapturesRequestedProduct()
        {
            var (service, distinctLoaded, _, testable) = BuildService(SinglePrice("prod1"));

            var prices = await service.EvaluateProductPricesAsync(Context("prod1"));

            Assert.Equal(10, prices.Single().List);
            Assert.Equal(1, distinctLoaded());
            Assert.Contains("prod1", testable.LoadBatches.SelectMany(x => x));
        }

        // Deferred from Task 3a — now that the cache routes through GetCachedProductPricesAsync,
        // a second identical eval must be served from cache: no new LoadBatches entry (AC-6 base case).
        [Fact]
        public async Task EvaluateProductPricesAsync_WarmCache_LoadsEachProductOnce()
        {
            var (service, distinctLoaded, batchCount, _) = BuildService(SinglePrice("prod1"));

            await service.EvaluateProductPricesAsync(Context("prod1"));
            await service.EvaluateProductPricesAsync(Context("prod1"));

            Assert.Equal(1, distinctLoaded());
            Assert.Equal(1, batchCount());
        }

        [Fact]
        public async Task EvaluateProductPricesAsync_OverlappingProductSets_LoadEachOnce()
        {
            var (service, distinctLoaded, _, _) = BuildService(new[]
            {
                SinglePrice("prod1").Single(),
                SinglePrice("prod2").Single(),
            });

            await service.EvaluateProductPricesAsync(Context("prod1"));
            await service.EvaluateProductPricesAsync(Context("prod1", "prod2"));

            Assert.Equal(2, distinctLoaded());
        }

        [Fact]
        public async Task EvaluateProductPricesAsync_ConcurrentColdMiss_LoadsOnce()
        {
            var (service, distinctLoaded, batchCount, _) = BuildService(SinglePrice("prod1"));

            var tasks = Enumerable.Range(0, 20)
                .Select(_ => service.EvaluateProductPricesAsync(Context("prod1")))
                .ToArray();
            await Task.WhenAll(tasks);

            Assert.Equal(1, distinctLoaded());
            Assert.Equal(1, batchCount());
        }

        [Fact]
        public async Task EvaluateProductPricesAsync_ProductWithoutPrices_NegativeCached()
        {
            var (service, _, batchCount, _) = BuildService(SinglePrice("prod1"));

            var first = await service.EvaluateProductPricesAsync(Context("prod2"));
            var second = await service.EvaluateProductPricesAsync(Context("prod2"));

            Assert.Empty(first);
            Assert.Empty(second);
            Assert.Equal(1, batchCount());
        }

        [Fact]
        public async Task EvaluateProductPricesAsync_WarmResult_IsNotSharedInstance()
        {
            var (service, _, _, _) = BuildService(SinglePrice("prod1"));

            var first = await service.EvaluateProductPricesAsync(Context("prod1"));
            var second = await service.EvaluateProductPricesAsync(Context("prod1"));

            var firstPrice = first.Single();
            var secondPrice = second.Single();

            Assert.False(ReferenceEquals(firstPrice, secondPrice));

            firstPrice.List = 999;

            Assert.NotEqual(999, secondPrice.List);
        }

        // Sibling of WarmResult_IsNotSharedInstance: that test only proves two returned instances
        // differ, which would still pass if a store/read branch skipped cloning as long as BOTH
        // branches skipped it identically. This proves the CACHED instance itself survives caller
        // mutation of a previously-returned Price — clone-on-read protects the cache, not just the caller.
        [Fact]
        public async Task EvaluateProductPricesAsync_MutatingReturnedPrice_DoesNotCorruptCache()
        {
            var (service, _, _, _) = BuildService(SinglePrice("prod1"));

            await service.EvaluateProductPricesAsync(Context("prod1")); // cold load, populates cache
            var second = await service.EvaluateProductPricesAsync(Context("prod1")); // warm clone

            second.Single().List = 999; // caller mutates its own clone

            var third = await service.EvaluateProductPricesAsync(Context("prod1")); // warm clone, again

            Assert.Equal(10, third.Single().List); // cache-stored value untouched by the caller's mutation
        }
    }

    // Definition only — see the usage note on PriceEvaluationCacheTests above.
    [CollectionDefinition(nameof(PriceEvaluationCacheCollection))]
    public class PriceEvaluationCacheCollection
    {
    }
}
