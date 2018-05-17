using Newtonsoft.Json;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher
{
    class ValueTypeFetcher<T> : IFetcher<T>
    {

        private readonly JsonSerializerFactory _serializerFactory;

        public ValueTypeFetcher(JsonSerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public T Fetch(JsonTextReader reader)
        {
            return _serializerFactory.Create().Deserialize<T>(reader);
        }
    }
}
