using Newtonsoft.Json;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher
{
    interface IFetcher<out T>
    {
        T Fetch(JsonTextReader reader);
    }
}
