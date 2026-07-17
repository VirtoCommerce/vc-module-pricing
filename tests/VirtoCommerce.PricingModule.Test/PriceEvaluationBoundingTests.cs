using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MockQueryable;
using Moq;
using VirtoCommerce.Platform.Core.Caching;
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

        // AC-14a: the default hook applies a 15-minute sliding expiration and sets no absolute bound.
        private sealed class TestableExpirationEvaluatorService : PricingEvaluatorService
        {
            public TestableExpirationEvaluatorService(ISettingsManager settingsManager)
                : base(() => null, null, null, null, new DefaultPricingPriorityFilterPolicy(), settingsManager)
            {
            }

            public void ApplyCacheEntryExpirationPublic(MemoryCacheEntryOptions options) => ApplyCacheEntryExpiration(options);
        }

        [Fact]
        public void Bounding_DefaultHook_SetsSlidingTtl()
        {
            var settings = PriceEvaluationCacheTests.CreateSettingsMock();
            var service = new TestableExpirationEvaluatorService(settings.Object);
            var options = new MemoryCacheEntryOptions();

            service.ApplyCacheEntryExpirationPublic(options);

            Assert.Equal(TimeSpan.FromMinutes(15), options.SlidingExpiration);
            Assert.Null(options.AbsoluteExpirationRelativeToNow);
        }

        // AC-14b: a subclass overriding the hook is invoked, and its options are the ones applied —
        // asserted via the hook seam (spy), since MemoryCache exposes no options read-back.
        private sealed class SpyExpirationEvaluatorService : PricingEvaluatorService
        {
            private readonly TimeSpan _absoluteExpiration;

            public MemoryCacheEntryOptions CapturedOptions { get; private set; }

            public SpyExpirationEvaluatorService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
                ISettingsManager settingsManager, PriceEvaluationCache priceEvaluationCache, TimeSpan absoluteExpiration)
                : base(repositoryFactory, null, null, platformMemoryCache, new DefaultPricingPriorityFilterPolicy(), settingsManager, priceEvaluationCache)
            {
                _absoluteExpiration = absoluteExpiration;
            }

            protected override void ApplyCacheEntryExpiration(MemoryCacheEntryOptions options)
            {
                options.AbsoluteExpirationRelativeToNow = _absoluteExpiration;
                CapturedOptions = options;
            }
        }

        [Fact]
        public async Task Bounding_OverriddenHook_IsUsed()
        {
            var mockPrices = SingleRowEach("p1").BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);

            var settings = PriceEvaluationCacheTests.CreateSettingsMock();
            var cache = new PriceEvaluationCache(settings.Object);
            var absoluteExpiration = TimeSpan.FromHours(2);
            var service = new SpyExpirationEvaluatorService(() => mock.Object, PriceEvaluationCacheTests.CreateCache(), settings.Object, cache, absoluteExpiration);

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1"));

            Assert.NotNull(service.CapturedOptions);
            Assert.Equal(absoluteExpiration, service.CapturedOptions.AbsoluteExpirationRelativeToNow);
            Assert.Null(service.CapturedOptions.SlidingExpiration);
        }

        // AC-15 (FromCurrentDateOnly): private helpers + tests below.

        private static (PricingEvaluatorService service, PriceEvaluationCacheTests.TestablePricingEvaluatorService testable, PriceEvaluationCache cache)
            BuildFromCurrentDateOnlyService(PriceEntity[] prices, bool fromCurrentDateOnly = true)
        {
            var mockPrices = prices.BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);

            var settings = PriceEvaluationCacheTests.CreateSettingsMock(fromCurrentDateOnly: fromCurrentDateOnly);
            var cache = new PriceEvaluationCache(settings.Object);
            var svc = new PriceEvaluationCacheTests.TestablePricingEvaluatorService(() => mock.Object, PriceEvaluationCacheTests.CreateCache(), settings.Object, cache);

            return (svc, svc, cache);
        }

        // Recomputes the same memKey the production code builds, so tests can probe the private
        // cache directly (GetType() must be the CONCRETE runtime type used at the call site — the
        // testable subclass here, not PricingEvaluatorService itself).
        private static bool TryGetCachedRows(PricingEvaluatorService service, PriceEvaluationCache cache, string pricelistId, string productId, out CachedPriceRows cached)
        {
            var memKey = CacheKey.Normalize(
                CacheKey.With(service.GetType(), nameof(PricingEvaluatorService.EvaluateProductPricesAsync), PriceEvaluationCacheKey.TokenKey(pricelistId, productId)));

            return cache.Cache.TryGetValue(memKey, out cached);
        }

        // AC-15: historical rows are dropped SQL-side at population — the collapsed entry holds only
        // the current row, and a current-date eval returns the current price.
        [Fact]
        public async Task FromCurrentDateOnly_DropsHistoricalRows()
        {
            var now = DateTime.UtcNow;
            var prices = new[]
            {
                new PriceEntity { Id = "p1-hist", List = 5, PricelistId = "List1", ProductId = "p1", EndDate = now.AddDays(-1) },
                new PriceEntity { Id = "p1-cur", List = 10, PricelistId = "List1", ProductId = "p1" },
            };
            var (service, _, cache) = BuildFromCurrentDateOnlyService(prices);

            var result = await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1"));

            Assert.True(TryGetCachedRows(service, cache, "List1", "p1", out var cached));
            Assert.Single(cached.Rows);
            Assert.Equal(10, cached.Rows[0].List);

            Assert.Equal(10, result.Single().List);
        }

        // AC-15: warm once, evaluate the same product at several Quantity values — each must be
        // correct from the SAME collapsed entry, with no reload triggered by the quantity change.
        [Fact]
        public async Task FromCurrentDateOnly_WarmAcrossQuantities()
        {
            var prices = new[]
            {
                new PriceEntity { Id = "p1-tier", List = 20, PricelistId = "List1", ProductId = "p1", MinQuantity = 5 },
            };
            var (service, testable, _) = BuildFromCurrentDateOnlyService(prices);

            var contextQty1 = PriceEvaluationCacheTests.Context("p1");
            contextQty1.Quantity = 1;
            var atQty1 = await service.EvaluateProductPricesAsync(contextQty1); // cold, warms the entry
            Assert.Empty(atQty1); // below the tier's MinQuantity

            var batchesAfterWarm = testable.LoadBatches.Count;

            var contextQty5 = PriceEvaluationCacheTests.Context("p1");
            contextQty5.Quantity = 5;
            var atQty5 = await service.EvaluateProductPricesAsync(contextQty5);
            Assert.Equal(20, atQty5.Single().List);

            var contextQty10 = PriceEvaluationCacheTests.Context("p1");
            contextQty10.Quantity = 10;
            var atQty10 = await service.EvaluateProductPricesAsync(contextQty10);
            Assert.Equal(20, atQty10.Single().List);

            Assert.Equal(batchesAfterWarm, testable.LoadBatches.Count); // no reload across quantities
        }

        // AC-15: a future-StartDate row is cached at population (not expired, so the SQL-side
        // collapse keeps it); an eval at a CertainDate at/after that start activates it purely via
        // the existing in-memory date filter, with no reload.
        [Fact]
        public async Task FromCurrentDateOnly_FutureStartRow_Activates()
        {
            var futureStart = DateTime.UtcNow.AddDays(10);
            var prices = new[]
            {
                new PriceEntity { Id = "p1-future", List = 15, PricelistId = "List1", ProductId = "p1", StartDate = futureStart },
            };
            var (service, testable, _) = BuildFromCurrentDateOnlyService(prices);

            var beforeStart = await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1")); // warms
            Assert.Empty(beforeStart); // not yet active

            var batchesAfterWarm = testable.LoadBatches.Count;

            var context = PriceEvaluationCacheTests.Context("p1");
            context.CertainDate = futureStart.AddDays(1);
            var afterStart = await service.EvaluateProductPricesAsync(context);

            Assert.Equal(15, afterStart.Single().List);
            Assert.Equal(batchesAfterWarm, testable.LoadBatches.Count); // served from the same collapsed entry
        }

        // [C2] AC-15 WARM bypass: warm, then evaluate at a CertainDate earlier than the entry's
        // LoadTime — must bypass to a fresh unfiltered load and must NOT repopulate the collapsed entry.
        [Fact]
        public async Task FromCurrentDateOnly_HistoricalDate_Bypasses()
        {
            var historicalDate = DateTime.UtcNow.AddYears(-1);
            var prices = new[]
            {
                // Expired long before "now" — dropped by the SQL-side collapse at population time,
                // but still valid at the historical eval date below.
                new PriceEntity
                {
                    Id = "p1-hist", List = 7, PricelistId = "List1", ProductId = "p1",
                    StartDate = historicalDate.AddDays(-10), EndDate = historicalDate.AddDays(10),
                },
                new PriceEntity { Id = "p1-cur", List = 20, PricelistId = "List1", ProductId = "p1" },
            };
            var (service, testable, cache) = BuildFromCurrentDateOnlyService(prices);

            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1")); // warms; collapses out p1-hist

            Assert.True(TryGetCachedRows(service, cache, "List1", "p1", out var warmEntry));
            Assert.Single(warmEntry.Rows);

            var batchesBeforeBypass = testable.LoadBatches.Count;

            var historicalContext = PriceEvaluationCacheTests.Context("p1");
            historicalContext.CertainDate = historicalDate;
            var result = await service.EvaluateProductPricesAsync(historicalContext);

            Assert.Equal(7, result.Single().List); // the historical row, invisible to the collapsed entry
            Assert.True(testable.LoadBatches.Count > batchesBeforeBypass); // bypass == a fresh DB load

            Assert.True(TryGetCachedRows(service, cache, "List1", "p1", out var entryAfterBypass));
            Assert.Equal(warmEntry.Rows.Length, entryAfterBypass.Rows.Length);
            Assert.Equal(warmEntry.LoadTime, entryAfterBypass.LoadTime); // unchanged — not repopulated
        }

        // [C2] The blocker case: EMPTY cache, evaluate a product with an expired row at a historical
        // CertainDate. A collapse-populate here would drop the historical row (wrong price); the
        // bypass must load unfiltered instead, and must not populate a collapsed entry for the pair.
        [Fact]
        public async Task FromCurrentDateOnly_ColdMiss_HistoricalDate_LoadsUnfiltered()
        {
            var historicalDate = DateTime.UtcNow.AddYears(-1);
            var prices = new[]
            {
                new PriceEntity
                {
                    Id = "p1-hist", List = 7, PricelistId = "List1", ProductId = "p1",
                    StartDate = historicalDate.AddDays(-10), EndDate = historicalDate.AddDays(10),
                },
            };
            var (service, _, cache) = BuildFromCurrentDateOnlyService(prices);

            var context = PriceEvaluationCacheTests.Context("p1");
            context.CertainDate = historicalDate;
            var result = await service.EvaluateProductPricesAsync(context);

            Assert.Equal(7, result.Single().List); // [C2] the historical row survived — the load was unfiltered
            Assert.False(TryGetCachedRows(service, cache, "List1", "p1", out _)); // never collapse-populated
        }

        // Regression: FromCurrentDateOnly=false must still reproduce the full 7-date anchor
        // (10/3/4/2/1/4/4), i.e. AC-5b's complete-unfiltered-set guarantee holds in default mode.
        [Fact]
        public async Task Default_Mode_StillFullSet()
        {
            var prices = new[]
            {
                new PriceEntity { Id = "1", List = 1, EndDate = new DateTime(2018, 09, 10, 0, 0, 0, 0, DateTimeKind.Utc), PricelistId = "List1", ProductId = "p1" },
                new PriceEntity { Id = "2", List = 2, StartDate = new DateTime(2018, 09, 15, 0, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2018, 09, 17, 0, 0, 0, 0, DateTimeKind.Utc), PricelistId = "List1", ProductId = "p1" },
                new PriceEntity { Id = "3", List = 3, StartDate = new DateTime(2018, 09, 26, 0, 0, 0, 0, DateTimeKind.Utc), EndDate = new DateTime(2018, 09, 29, 0, 0, 0, 0, DateTimeKind.Utc), PricelistId = "List1", ProductId = "p1" },
                new PriceEntity { Id = "4", List = 4, StartDate = new DateTime(2018, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), PricelistId = "List1", ProductId = "p1" },
                new PriceEntity { Id = "10", List = 10, PricelistId = "List1", ProductId = "p1" },
            };
            var (service, _, _) = BuildFromCurrentDateOnlyService(prices, fromCurrentDateOnly: false);

            var context = PriceEvaluationCacheTests.Context("p1");

            context.CertainDate = new DateTime(2018, 09, 20, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(10, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = new DateTime(2018, 09, 27, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(3, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = new DateTime(2118, 10, 2, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(4, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = new DateTime(2018, 9, 16, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(2, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = new DateTime(2018, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            Assert.Equal(1, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = DateTime.UtcNow;
            Assert.Equal(4, (await service.EvaluateProductPricesAsync(context)).Single().List);

            context.CertainDate = null;
            Assert.Equal(4, (await service.EvaluateProductPricesAsync(context)).Single().List);
        }

        // M3-bounding: proves the four new instruments actually move — a forced compaction eviction,
        // an AC-15 historical bypass, and a single-entry oversize pair (rows.Length alone > RowLimit)
        // — and that the oversize pair still returns its rows (graceful degradation, not OOM).
        [Fact]
        public async Task Bounding_EmitsEvictionAndBypassCounters()
        {
            var counts = new Dictionary<string, long>();

            using var listener = new MeterListener();
            listener.InstrumentPublished = (instrument, meterListener) =>
            {
                if (instrument.Meter.Name == "VirtoCommerce.PricingModule"
                    && (instrument.Name == "pricing.evaluator.cache.evictions"
                        || instrument.Name == "pricing.evaluator.cache.oversize_rejected"
                        || instrument.Name == "pricing.evaluator.cache.date_bypass"))
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

            var now = DateTime.UtcNow;
            var prices = new[]
            {
                // (a) eviction trio — single row each, RowLimit below is set to 2.
                new PriceEntity { Id = "p1-p", List = 10, PricelistId = "List1", ProductId = "p1" },
                new PriceEntity { Id = "p2-p", List = 10, PricelistId = "List1", ProductId = "p2" },
                new PriceEntity { Id = "p3-p", List = 10, PricelistId = "List1", ProductId = "p3" },
                // (b) date-bypass pair — historical row collapsed at population, current row kept.
                new PriceEntity { Id = "hist-old", List = 5, PricelistId = "List1", ProductId = "hist", EndDate = now.AddDays(-1) },
                new PriceEntity { Id = "hist-cur", List = 10, PricelistId = "List1", ProductId = "hist" },
                // (c) oversize product — 3 rows for one pair alone exceed RowLimit=2.
                new PriceEntity { Id = "big-1", List = 1, PricelistId = "List1", ProductId = "big" },
                new PriceEntity { Id = "big-2", List = 2, PricelistId = "List1", ProductId = "big" },
                new PriceEntity { Id = "big-3", List = 3, PricelistId = "List1", ProductId = "big" },
            };
            var mockPrices = prices.BuildMock();
            var mock = new Mock<IPricingRepository>();
            mock.SetupGet(x => x.Prices).Returns(mockPrices);

            var settings = PriceEvaluationCacheTests.CreateSettingsMock(rowLimit: 2, fromCurrentDateOnly: true);
            var cache = new PriceEvaluationCache(settings.Object);
            var service = new PriceEvaluationCacheTests.TestablePricingEvaluatorService(() => mock.Object, PriceEvaluationCacheTests.CreateCache(), settings.Object, cache);

            // (a) eviction: warm 3 single-row entries past RowLimit=2, force compaction.
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p1"));
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p2"));
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("p3"));
            ((MemoryCache)cache.Cache).Compact(0.5); // [I4] force async compaction synchronously

            // (b) date_bypass: warm, then re-evaluate at a historical CertainDate earlier than LoadTime.
            await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("hist"));
            var historicalContext = PriceEvaluationCacheTests.Context("hist");
            historicalContext.CertainDate = now.AddYears(-1);
            await service.EvaluateProductPricesAsync(historicalContext);

            // (c) oversize_rejected: "big"'s own row count (3) exceeds RowLimit (2) — must not throw,
            // must not be cached, and must still return rows to the caller.
            var oversizeResult = await service.EvaluateProductPricesAsync(PriceEvaluationCacheTests.Context("big"));

            Assert.True(counts.GetValueOrDefault("pricing.evaluator.cache.evictions") >= 1);
            Assert.True(counts.GetValueOrDefault("pricing.evaluator.cache.date_bypass") >= 1);
            Assert.True(counts.GetValueOrDefault("pricing.evaluator.cache.oversize_rejected") >= 1);
            Assert.NotEmpty(oversizeResult);
        }
    }
}
