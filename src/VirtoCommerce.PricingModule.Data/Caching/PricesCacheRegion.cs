using System;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricesCacheRegion : CancellableCacheRegion<PricesCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string priceId)
        {
            if (string.IsNullOrEmpty(priceId))
            {
                throw new ArgumentNullException(nameof(priceId));
            }
            return CreateChangeTokenForKey(priceId);
        }

        public static void ExpirePrice(string priceId)
        {
            ExpireTokenForKey(priceId);
        }
    }
}
