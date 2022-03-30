using System;

namespace VirtoCommerce.PricingModule.Core.Model
{
    [Flags]
    public enum PriceListResponseGroup
    {
        NoDetails = 1,
        Full = 2
    }
}
