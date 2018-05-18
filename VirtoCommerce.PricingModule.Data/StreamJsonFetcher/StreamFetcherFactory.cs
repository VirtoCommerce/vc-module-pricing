using System.IO;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    internal class StreamFetcherFactory
    {

        private readonly JsonSerializerFactory _jsonSerializerFactory;

        public StreamFetcherFactory(JsonSerializerFactory jsonSerializerFactory)
        {
            _jsonSerializerFactory = jsonSerializerFactory;
        }

        public StreamFetcher Create(Stream stream)
        {
            var fethcher = new StreamFetcher(_jsonSerializerFactory, stream);

            return fethcher;
        }
    }
}
