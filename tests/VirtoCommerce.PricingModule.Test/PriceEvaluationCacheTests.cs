using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Search;
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
        // PlatformMemoryCache's ctor requires cache, options, and logger — no shorter overload exists.
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
            public TestablePricingEvaluatorService(Func<IPricingRepository> f, IPlatformMemoryCache c, ISettingsManager s, IItemService p = null)
                : base(f, p, null, c, new DefaultPricingPriorityFilterPolicy(), s) { }

            protected override Task<IList<Price>> LoadPricesFromDatabaseAsync(IList<string> productIds, IList<string> pricelistIds)
            {
                LoadBatches.Add(productIds.ToArray());
                return base.LoadPricesFromDatabaseAsync(productIds, pricelistIds);
            }
        }

        internal static PriceEntity[] SinglePrice(string productId) =>
            new[] { new PriceEntity { Id = productId + "-p", List = 10, PricelistId = "List1", ProductId = productId } };

        // Exposes the protected GetProductPrices(productIds) call site so the real-builder bypass
        // test can assert on the PriceEvaluationContext actually built and passed to the evaluator,
        // rather than on a hand-set flag on a context the test constructs itself.
        private sealed class TestableProductPriceDocumentBuilder : ProductPriceDocumentBuilder
        {
            public TestableProductPriceDocumentBuilder(IPricingEvaluatorService pricingEvaluatorService, ISettingsManager settingsManager, IProductSearchService productsSearchService)
                : base(pricingEvaluatorService, settingsManager, productsSearchService)
            {
            }

            public Task<IList<Price>> GetProductPricesPublic(IList<string> productIds) => GetProductPrices(productIds);
        }

        internal static PriceEvaluationContext Context(params string[] productIds) => new()
        {
            ProductIds = productIds,
            Pricelists = new[] { new Pricelist { Id = "List1", Priority = 0 } },
        };

        // dbCalls = cumulative DISTINCT products actually loaded from DB (NOT factory invocations).
        // internal (not private): shared with PriceEvaluationInvalidationTests so its per-key
        // invalidation-precision test can inspect testable.LoadBatches without duplicating this setup.
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

        // Overload for the variation-inheritance test: PostProcessPrices only recurses into
        // main-product inheritance when _productService is non-null.
        internal static (PricingEvaluatorService service, Func<int> distinctLoaded, Func<int> batchCount, TestablePricingEvaluatorService testable) BuildService(PriceEntity[] prices, IItemService productService)
        {
            var mockPrices = prices.BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);
            var settings = new Mock<ISettingsManager>();
            settings.Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = true });
            var svc = new TestablePricingEvaluatorService(() => mock.Object, CreateCache(), settings.Object, productService);
            return (svc,
                () => svc.LoadBatches.SelectMany(x => x).Distinct().Count(),
                () => svc.LoadBatches.Count,
                svc);
        }

        // Load-recorder infra proof: captures the requested product on a single (cold) eval.
        [Fact]
        public async Task EvaluateProductPricesAsync_LoadRecorder_CapturesRequestedProduct()
        {
            var (service, distinctLoaded, _, testable) = BuildService(SinglePrice("prod1"));

            var prices = await service.EvaluateProductPricesAsync(Context("prod1"));

            Assert.Equal(10, prices.Single().List);
            Assert.Equal(1, distinctLoaded());
            Assert.Contains("prod1", testable.LoadBatches.SelectMany(x => x));
        }

        // A second identical eval must be served from cache: no new LoadBatches entry.
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

        // O(N) proof: simulates a cart build-up (N sequential evals over cumulative product sets),
        // sharing one cache. A broken/uncached impl would load N(N+1)/2 product ids total; this
        // asserts the O(N) shape directly on testable.LoadBatches.
        [Fact]
        public async Task EvaluateProductPricesAsync_BuildUp_LoadsEachProductExactlyOnce()
        {
            const int n = 8;
            var prices = Enumerable.Range(1, n)
                .Select(i => new PriceEntity { Id = $"p{i}-p", List = 10, PricelistId = "List1", ProductId = $"p{i}" })
                .ToArray();
            var (service, distinctLoaded, _, testable) = BuildService(prices);

            for (var i = 1; i <= n; i++)
            {
                await service.EvaluateProductPricesAsync(Context(Enumerable.Range(1, i).Select(k => $"p{k}").ToArray()));
            }

            // O(N): each product loaded once total, not re-loaded on every subsequent add.
            Assert.Equal(n, distinctLoaded());
            // Stronger: total product ids ever sent to DB == n (would be n(n+1)/2 without the cache).
            Assert.Equal(n, testable.LoadBatches.SelectMany(x => x).Count());
        }

        // A partially-warm request must not re-load the already-cached product.
        // OverlappingProductSets_LoadEachOnce (above) only proves cumulative distinct-loaded count;
        // this asserts the SECOND eval's own DB batch directly.
        [Fact]
        public async Task EvaluateProductPricesAsync_PartialMiss_LoadsOnlyUncached()
        {
            var (service, _, _, testable) = BuildService(new[]
            {
                SinglePrice("prod1").Single(),
                SinglePrice("prod2").Single(),
            });

            await service.EvaluateProductPricesAsync(Context("prod1")); // warms prod1

            await service.EvaluateProductPricesAsync(Context("prod1", "prod2"));

            Assert.Equal(new[] { "prod2" }, testable.LoadBatches.Last());
        }

        // PostProcessPrices' variation-inheritance recursion re-enters EvaluateProductPricesAsync
        // for the main product id — that recursive call must be served from cache too, not treated
        // as a fresh, uncached load.
        [Fact]
        public async Task EvaluateProductPricesAsync_VariationInheritsCachedMainProduct_NoExtraLoad()
        {
            var productService = new Mock<IItemService>();
            productService
                .Setup(x => x.GetAsync(It.Is<IList<string>>(ids => ids.Contains("variation1")), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new List<CatalogProduct> { new() { Id = "variation1", MainProductId = "mainProd" } });

            var (service, _, _, testable) = BuildService(SinglePrice("mainProd"), productService.Object);

            await service.EvaluateProductPricesAsync(Context("mainProd")); // warms the main product
            var batchesBeforeVariationEval = testable.LoadBatches.Count;

            await service.EvaluateProductPricesAsync(Context("variation1"));

            var newBatches = testable.LoadBatches.Skip(batchesBeforeVariationEval).ToList();
            Assert.Single(newBatches);
            Assert.Equal(new[] { "variation1" }, newBatches[0]);
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

        // Proves the hit/miss counters on PricingEvaluatorService's static
        // "VirtoCommerce.PricingModule" Meter actually move. Asserts only THIS test's own
        // MeterListener accumulation (not global totals) — the counters are process-static and
        // shared across the assembly; [Collection] on this class serializes it against its sibling
        // PriceEvaluationInvalidationTests, but other test classes could still run concurrently.
        [Fact]
        public async Task EvaluateProductPricesAsync_EmitsHitAndMissCounters()
        {
            var counts = new Dictionary<string, long>();

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == "VirtoCommerce.PricingModule"
                    && (instrument.Name == "pricing.evaluator.cache.hits" || instrument.Name == "pricing.evaluator.cache.misses"))
                {
                    meterListener.EnableMeasurementEvents(instrument);
                }
            };
            listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
            {
                lock (counts)
                {
                    counts[instrument.Name] = counts.GetValueOrDefault(instrument.Name) + measurement;
                }
            });
            listener.Start();

            var (service, _, _, _) = BuildService(SinglePrice("prod1"));

            await service.EvaluateProductPricesAsync(Context("prod1")); // cold eval -> miss

            Assert.True(counts.GetValueOrDefault("pricing.evaluator.cache.misses") >= 1);

            await service.EvaluateProductPricesAsync(Context("prod1")); // warm eval -> hit

            Assert.True(counts.GetValueOrDefault("pricing.evaluator.cache.hits") >= 1);
        }

        // A caller that opts out of the evaluator cache must never be served a cached read —
        // both evals of the same product hit the DB, unlike the warm-cache base case above.
        [Fact]
        public async Task EvaluateProductPricesAsync_Bypass_AlwaysLoadsFresh()
        {
            var (service, _, batchCount, testable) = BuildService(SinglePrice("prod1"));

            var firstContext = Context("prod1");
            firstContext.BypassEvaluatorCache = true;
            await service.EvaluateProductPricesAsync(firstContext);

            var secondContext = Context("prod1");
            secondContext.BypassEvaluatorCache = true;
            await service.EvaluateProductPricesAsync(secondContext);

            Assert.Equal(2, batchCount());
            Assert.Equal(new[] { "prod1" }, testable.LoadBatches[0]);
            Assert.Equal(new[] { "prod1" }, testable.LoadBatches[1]);
        }

        // Output-shape parity for Pricelist hydration. PriceEntity.ToModel() does NOT populate
        // Price.Pricelist (no such navigation on the model) — it only derives price.Currency from the
        // Included PricelistEntity. The uncached original behaves the same, so the correct parity
        // assertion is Currency hydration, not Price.Pricelist != null. Proves LoadPricesFromDatabaseAsync's
        // Include(x => x.Pricelist) survives both the cold DB read and the warm cache store/clone-on-read
        // (CloneTyped()) round trip.
        [Fact]
        public async Task EvaluateProductPricesAsync_ReturnedPrice_HasCurrencyHydratedAndCachePreservesIt()
        {
            var (service, _, batchCount, _) = BuildService(new[]
            {
                new PriceEntity
                {
                    Id = "p",
                    List = 10,
                    PricelistId = "List1",
                    ProductId = "prod1",
                    Pricelist = new PricelistEntity { Id = "List1", Currency = "USD" },
                },
            });

            var cold = await service.EvaluateProductPricesAsync(Context("prod1"));
            Assert.Equal("USD", cold.Single().Currency);

            var warm = await service.EvaluateProductPricesAsync(Context("prod1"));
            Assert.Equal("USD", warm.Single().Currency);
            Assert.Equal(1, batchCount());
        }

        // Proves the flag is actually set at the production call site inside
        // ProductPriceDocumentBuilder.GetProductPrices — a spy IPricingEvaluatorService captures the
        // PriceEvaluationContext the REAL builder builds and passes down, so this fails if the builder
        // ever stops setting BypassEvaluatorCache, unlike a unit test that sets the flag by hand.
        [Fact]
        public async Task ProductPriceDocumentBuilder_GetProductPrices_BypassesCache()
        {
            PriceEvaluationContext captured = null;
            var evaluatorMock = new Mock<IPricingEvaluatorService>();
            evaluatorMock
                .Setup(x => x.EvaluateProductPricesAsync(It.IsAny<PriceEvaluationContext>()))
                .Callback<PriceEvaluationContext>(ctx => captured = ctx)
                .ReturnsAsync(new List<Price>());

            var builder = new TestableProductPriceDocumentBuilder(
                evaluatorMock.Object,
                new Mock<ISettingsManager>().Object,
                new Mock<IProductSearchService>().Object);

            await builder.GetProductPricesPublic(new[] { "prod1" });

            Assert.NotNull(captured);
            Assert.True(captured.BypassEvaluatorCache);
        }
    }

    // Definition only — see the usage note on PriceEvaluationCacheTests above.
    [CollectionDefinition(nameof(PriceEvaluationCacheCollection))]
    public class PriceEvaluationCacheCollection
    {
    }
}
