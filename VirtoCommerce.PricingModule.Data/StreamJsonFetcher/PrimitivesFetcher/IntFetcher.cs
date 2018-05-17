using Newtonsoft.Json;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher
{
    class IntFetcher : IFetcher<int>
    {

        public int Fetch(JsonTextReader reader)
        {
            int.TryParse(reader.Value.ToString(), out var result);

            return result;
        }
    }
}
