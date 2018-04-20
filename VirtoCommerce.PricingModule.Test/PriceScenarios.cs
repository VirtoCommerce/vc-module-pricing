using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{
    public class PriceScenarios
    {
        [Fact]
        public void Can_return_pricelists()
        {
            var evalContext = new Domain.Pricing.Model.PriceEvaluationContext
            {
                ProductIds = new[] { "4ed55441810a47da88a483e5a1ee4e94" }
            };

            var pricingService = GetPricingService(GetPricingRepository);
            var priceLists = pricingService.EvaluatePriceLists(evalContext);
            Assert.True(priceLists.Count() > 0);
            var prices = pricingService.EvaluateProductPrices(evalContext);
            Assert.True(prices.Count() > 0);
        }

        [Fact]
        public void Can_return_prices_from_many_pricelists_by_priority()
        {
            var evalContext = new Domain.Pricing.Model.PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                PricelistIds = new[] { "Pricelist 1", "Pricelist 2", "Pricelist 3" }
            };

            var mockPrices = new[] {
                new PriceEntity { List = 10, MinQuantity = 2, PricelistId = "Pricelist 1" },

                new PriceEntity { List = 9, MinQuantity = 1, PricelistId = "Pricelist 2" },
                new PriceEntity { List = 10, MinQuantity = 2, PricelistId = "Pricelist 2" },

                new PriceEntity { List = 6, MinQuantity = 2, PricelistId = "Pricelist 3" },
                new PriceEntity { List = 5, MinQuantity = 3,  PricelistId = "Pricelist 3" }                
            };
            for (int i = 0; i < mockPrices.Length; i++)
            {
                mockPrices[i].Id = i.ToString();
                mockPrices[i].ProductId = "ProductId";
            }
            var pricingService = GetPricingService(() => GetPricingRepositoryMock(mockPrices));

            var prices = pricingService.EvaluateProductPrices(evalContext).ToArray();

            // only 2 prices (from higher priority pricelists) returned, but not for MinQuantity == 3
            Assert.Equal(2, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[0].Id, prices[1].Id);
            Assert.DoesNotContain(prices, x => x.MinQuantity == 3);

            // Pricelist priority changed
            evalContext.PricelistIds = new[] { "Pricelist 3", "Pricelist 2", "Pricelist 1" };
            prices = pricingService.EvaluateProductPrices(evalContext).ToArray();

            // 3 prices returned, but not from "Pricelist 1"
            Assert.Equal(3, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[3].Id, prices[1].Id);
            Assert.Equal(mockPrices[4].Id, prices[2].Id);
            Assert.DoesNotContain(prices, x => x.PricelistId == "Pricelist 1");
        }

        private IPricingService GetPricingService(Func<IPricingRepository> repositoryFactory)
        {
            var logger = new Moq.Mock<ILog>();
            return new PricingServiceImpl(repositoryFactory, null, logger.Object, null, null, null);
        }

        private IPricingRepository GetPricingRepository()
        {
            var result = new PricingRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null));
            return result;
        }

        private IPricingRepository GetPricingRepositoryMock(IEnumerable<PriceEntity> prices)
        {
            var mock = new Moq.Mock<IPricingRepository>();
            mock.Setup(foo => foo.Prices).Returns(prices.AsQueryable());
            return mock.Object;
        }
    }
}
