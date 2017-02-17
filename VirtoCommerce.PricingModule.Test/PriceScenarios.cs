using CacheManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;
using Common.Logging;

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

            var pricingService = GetPricingService();
            var priceLists = pricingService.EvaluatePriceLists(evalContext);
            Assert.True(priceLists.Count() > 0);
            var prices = pricingService.EvaluateProductPrices(evalContext);
            Assert.True(prices.Count() > 0);
        }

        private IPricingService GetPricingService()
        {
            var logger = new Moq.Mock<ILog>();
            return new PricingServiceImpl(GetPricingRepository, null, logger.Object, null, null, null);
        }

        private IPricingRepository GetPricingRepository()
        {
            var result = new PricingRepositoryImpl("VirtoCommerce", new EntityPrimaryKeyGeneratorInterceptor(), new AuditableInterceptor(null));
            return result;
        }
    }
}
