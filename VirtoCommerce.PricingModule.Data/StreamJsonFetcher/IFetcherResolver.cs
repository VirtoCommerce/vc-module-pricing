using VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    interface IFetcherResolver
    {
        IFetcher<T> Resolve<T>();
    }
}
