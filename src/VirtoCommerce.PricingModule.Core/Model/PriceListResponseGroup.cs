using System;

namespace VirtoCommerce.PricingModule.Core.Model
{
    [Flags]
    public enum PriceListResponseGroup
    {
        /// <summary>
        /// Skip details composed into the pricelist (i.e. price list assignments) to speed up pricelists enlisting 
        /// </summary>
        NoDetails = 1,
        Full = 2
    }
}
