using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricelistsCacheRegion : CancellableCacheRegion<PricelistsCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _entityRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static IChangeToken CreateChangeToken(string[] entityIds)
        {
            if (entityIds == null)
            {
                throw new ArgumentNullException(nameof(entityIds));
            }

            var changeTokens = new List<IChangeToken> { CreateChangeToken() };
            foreach (var entityId in entityIds)
            {
                changeTokens.Add(new CancellationChangeToken(_entityRegionTokenLookup.GetOrAdd(entityId, new CancellationTokenSource()).Token));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpirePricelist(string pricelistId)
        {
            if (_entityRegionTokenLookup.TryRemove(pricelistId, out var token))
            {
                token.Cancel();
            }
        }
    }
}
