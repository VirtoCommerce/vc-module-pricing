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
        public void Can_return_prices_by_priority()
        {
            var evalContext = new Domain.Pricing.Model.PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                PricelistIds = new[] { "Pricelist 1", "Pricelist 2" }
            };

            var mockPrices = new[] {
                new PriceEntity { List = 11, MinQuantity = 10, PricelistId = "Pricelist 2" },
                new PriceEntity { List = 11, MinQuantity = 15, PricelistId = "Pricelist 2" },
                new PriceEntity { List = 11, MinQuantity = 44, PricelistId = "Pricelist 2" },
                new PriceEntity { List = 11, MinQuantity = 50, PricelistId = "Pricelist 2" },
                new PriceEntity { List = 99, MinQuantity = 1,  PricelistId = "Pricelist 2" },
                new PriceEntity { List = 95, MinQuantity = 5,  PricelistId = "Pricelist 2" },

                new PriceEntity { List = 55, MinQuantity = 10, PricelistId = "Pricelist 1" },
                new PriceEntity { List = 66, MinQuantity = 20, PricelistId = "Pricelist 1" },
                new PriceEntity { List = 44, MinQuantity = 44, PricelistId = "Pricelist 1" }
            };
            for (int i = 0; i < mockPrices.Length; i++)
            {
                mockPrices[i].Id = i.ToString();
                mockPrices[i].ProductId = "ProductId";
            }

            var pricingService = GetPricingService(() => GetPricingRepositoryMock(mockPrices));

            var prices = pricingService.EvaluateProductPrices(evalContext).ToArray();

            Assert.Equal(5, prices.Length);
            Assert.Equal(mockPrices[4].Id, prices[0].Id);
            Assert.Equal(mockPrices[5].Id, prices[1].Id);
            Assert.Equal(mockPrices[6].Id, prices[2].Id);
            Assert.Equal(mockPrices[7].Id, prices[3].Id);
            Assert.Equal(mockPrices[8].Id, prices[4].Id);

            // order changed
            evalContext.PricelistIds = new[] { "Pricelist 2", "Pricelist 1" };

            prices = pricingService.EvaluateProductPrices(evalContext).ToArray();

            Assert.Equal(6, prices.Length);
            Assert.Equal(mockPrices[4].Id, prices[0].Id);
            Assert.Equal(mockPrices[5].Id, prices[1].Id);
            Assert.Equal(mockPrices[0].Id, prices[2].Id);
            Assert.Equal(mockPrices[1].Id, prices[3].Id);
            Assert.Equal(mockPrices[2].Id, prices[4].Id);
            Assert.Equal(mockPrices[3].Id, prices[5].Id);
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
