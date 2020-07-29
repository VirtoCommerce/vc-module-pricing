using System;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricelistsCacheRegion : CancellableCacheRegion<PricelistsCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string pricelistId)
        {
            if (string.IsNullOrEmpty(pricelistId))
            {
                throw new ArgumentNullException(nameof(pricelistId));
            }
            return CreateChangeTokenForKey(pricelistId);
        }

        public static void ExpirePricelist(string pricelistId)
        {
            ExpireTokenForKey(pricelistId);
        }
    }
}
