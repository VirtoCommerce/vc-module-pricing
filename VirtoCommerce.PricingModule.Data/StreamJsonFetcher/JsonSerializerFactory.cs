using Newtonsoft.Json;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    internal class JsonSerializerFactory
    {
        public JsonSerializer Create()
        {
            return new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }
    }
}
