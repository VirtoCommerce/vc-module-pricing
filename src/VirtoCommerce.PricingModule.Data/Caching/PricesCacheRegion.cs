using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricesCacheRegion : CancellableCacheRegion<PricesCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string[] entityIds)
        {
            if (entityIds == null)
            {
                throw new ArgumentNullException(nameof(entityIds));
            }
            var changeTokens = new List<IChangeToken> { CreateChangeToken() };
            foreach (var entityId in entityIds)
            {
                changeTokens.Add(CreateChangeTokenForKey(priceId));
            }
            return new CompositeChangeToken(changeTokens);            
        }

        public static void ExpirePrice(string priceId)
        {
            ExpireTokenForKey(priceId);
        }
    }
}
