using VirtoCommerce.PricingModule.Data.StreamJsonFetcher;
using VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher;
using Xunit;

namespace VirtoCommerce.PricingModule.Tests
{
    public class FetchersResolverTest
    {

        private class Test
        {

        }

        [Fact]
        public void NotFoundPrimitiveFetcherTest()
        {
            var resolver = new PrimitiveFetcherResolver(new ValueTypeFetcherFactory(new JsonSerializerFactory()));

            var fetcher = resolver.Resolve<Test>();

            Assert.IsType<ValueTypeFetcher<Test>>(fetcher);
        }
    }
}
