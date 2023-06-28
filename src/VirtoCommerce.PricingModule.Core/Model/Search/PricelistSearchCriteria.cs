using System.Collections.Generic;

namespace VirtoCommerce.PricingModule.Core.Model.Search
{
    public class PricelistSearchCriteria : PricingSearchCriteria
    {
        public IList<string> Currencies { get; set; }
    }
}
