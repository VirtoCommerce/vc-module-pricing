using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricelistsCacheRegion : CancellableCacheRegion<PricelistsCacheRegion>
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
                changeTokens.Add(CreateChangeTokenForKey(entityId));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpirePricelist(string pricelistId)
        {
            ExpireTokenForKey(pricelistId);
        }
    }
}
