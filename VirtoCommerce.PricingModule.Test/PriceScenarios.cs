using System.Linq;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.PricingModule.Data.Services;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{
    public class PriceScenarios
    {

        [Fact]
        public void Can_return_prices_from_many_pricelists_by_priority()
        {
            var evalContext = new PriceEvaluationContext
            {
                ProductIds = new[] { "ProductId" },
                PricelistIds = new[] { "Pricelist 1", "Pricelist 2", "Pricelist 3" }
            };

            var mockPrices = new[] {
                new Price { Id = "1", List = 10, MinQuantity = 2, PricelistId = "Pricelist 1", ProductId = "ProductId" },

                new Price {  Id = "2",List = 9, MinQuantity = 1, PricelistId = "Pricelist 2", ProductId = "ProductId" },
                new Price {  Id = "3",List = 10, MinQuantity = 2, PricelistId = "Pricelist 2", ProductId = "ProductId" },

                new Price {  Id = "4",List = 6, MinQuantity = 2, PricelistId = "Pricelist 3", ProductId = "ProductId" },
                new Price { Id = "5", List = 5, MinQuantity = 3,  PricelistId = "Pricelist 3", ProductId = "ProductId" }
            };

            var prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            // only 2 prices (from higher priority pricelists) returned, but not for MinQuantity == 3
            Assert.Equal(2, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[0].Id, prices[1].Id);
            Assert.DoesNotContain(prices, x => x.MinQuantity == 3);

            // Pricelist priority changed
            evalContext.PricelistIds = new[] { "Pricelist 3", "Pricelist 2", "Pricelist 1" };
            prices = new DefaultPricingPriorityFilterPolicy().FilterPrices(mockPrices, evalContext).ToArray();

            // 3 prices returned, but not from "Pricelist 1"
            Assert.Equal(3, prices.Length);
            Assert.Equal(mockPrices[1].Id, prices[0].Id);
            Assert.Equal(mockPrices[3].Id, prices[1].Id);
            Assert.Equal(mockPrices[4].Id, prices[2].Id);
            Assert.DoesNotContain(prices, x => x.PricelistId == "Pricelist 1");
        }

    }
}
