using System;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.PricingModule.Data.Caching
{
    public class PricelistAssignmentsCacheRegion : CancellableCacheRegion<PricelistAssignmentsCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string pricelistAssignmentId)
        {
            if (string.IsNullOrEmpty(pricelistAssignmentId))
            {
                throw new ArgumentNullException(nameof(pricelistAssignmentId));
            }
            return CreateChangeTokenForKey(pricelistAssignmentId);
        }

        public static void ExpirePricelistAssignment(string pricelistAssignmentId)
        {
            ExpireTokenForKey(pricelistAssignmentId);
        }
    }
}
