using Newtonsoft.Json;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher
{
    class StringFetcher : IFetcher<string>
    {

        public string Fetch(JsonTextReader reader)
        {
            return reader.Value.ToString();
        }
    }
}
