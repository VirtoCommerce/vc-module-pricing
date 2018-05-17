using System.Collections.Generic;
using VirtoCommerce.PricingModule.Data.StreamJsonFetcher.PrimitivesFetcher;

namespace VirtoCommerce.PricingModule.Data.StreamJsonFetcher
{
    class PrimitiveFetcherResolver : IFetcherResolver
    {

        private readonly ValueTypeFetcherFactory _valueTypeFetcherFactory;

        public PrimitiveFetcherResolver(ValueTypeFetcherFactory valueTypeFetcherFactory)
        {
            _valueTypeFetcherFactory = valueTypeFetcherFactory;
        }

        private readonly IDictionary<string, object> _availableFetchers = new Dictionary<string, object>
        {
            { typeof(string).ToString(), new StringFetcher() },
            { typeof(int).ToString(), new IntFetcher() }
        };

        public IFetcher<T> Resolve<T>()
        {
            if (_availableFetchers.ContainsKey(typeof(T).ToString()))
            {
                return _availableFetchers[typeof(T).ToString()] as IFetcher<T>;
            }
            else
            {
                return _valueTypeFetcherFactory.Create<T>();
            }
        }
    }
}
