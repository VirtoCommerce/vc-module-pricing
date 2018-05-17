namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher
{
    class ValueTypeFetcherFactory
    {

        private readonly JsonSerializerFactory _serializerFactory;

        public ValueTypeFetcherFactory(JsonSerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
        }

        public IFetcher<T> Create<T>()
        {
            return new ValueTypeFetcher<T>(_serializerFactory);
        }
    }
}
