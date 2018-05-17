using System.IO;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    internal class StreamFetcherFactory
    {

        private readonly JsonSerializerFactory _jsonSerializerFactory;
        private readonly PrimitiveFetcherResolver _resolver;

        public StreamFetcherFactory(JsonSerializerFactory jsonSerializerFactory, PrimitiveFetcherResolver resolver)
        {
            _jsonSerializerFactory = jsonSerializerFactory;
            _resolver = resolver;
        }

        public StreamFetcher Create(Stream stream)
        {
            var fethcher = new StreamFetcher(_jsonSerializerFactory, stream, _resolver);

            return fethcher;
        }
    }
}
