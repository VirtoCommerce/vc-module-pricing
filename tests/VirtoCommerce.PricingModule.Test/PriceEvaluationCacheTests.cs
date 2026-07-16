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
    public class PriceEvaluationCacheTests
    {
        // 3-arg ctor confirmed against PricingEvaluatorServiceTests.cs:185.
        internal static IPlatformMemoryCache CreateCache() =>
            new PlatformMemoryCache(new MemoryCache(new MemoryCacheOptions()),
                Options.Create(new CachingOptions()),
                new Mock<Microsoft.Extensions.Logging.ILogger<PlatformMemoryCache>>().Object);

        // Records EXACTLY which product ids reach the DB — the only sound "read volume" metric.
        private sealed class TestablePricingEvaluatorService : PricingEvaluatorService
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
        private static (PricingEvaluatorService service, Func<int> distinctLoaded, Func<int> batchCount, TestablePricingEvaluatorService testable) BuildService(PriceEntity[] prices)
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
    }
}
