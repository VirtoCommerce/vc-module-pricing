using VirtoCommerce.PricingModule.Data.Caching;
using Xunit;

namespace VirtoCommerce.PricingModule.Test
{
    public class PriceEvaluationCacheKeyTests
    {
        [Fact]
        public void TokenKey_SamePair_IsStableAndDistinct()
        {
            var a = PriceEvaluationCacheKey.TokenKey("pl1", "prod1");
            var b = PriceEvaluationCacheKey.TokenKey("pl1", "prod1");
            var c = PriceEvaluationCacheKey.TokenKey("pl1", "prod2");

            Assert.Equal(a, b);
            Assert.NotEqual(a, c);
        }

        [Fact]
        public void TokenKey_NoDelimiterCollision()
        {
            // Unconstrained id strings must not alias across the delimiter boundary.
            Assert.NotEqual(
                PriceEvaluationCacheKey.TokenKey("pl1:", "prod1"),
                PriceEvaluationCacheKey.TokenKey("pl1", ":prod1"));
        }
    }
}
