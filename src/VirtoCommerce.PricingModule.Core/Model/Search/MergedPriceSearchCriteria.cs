using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PricingModule.Core.Model.Search
{
    public class MergedPriceSearchCriteria : SearchCriteriaBase
    {
        public bool All { get; set; }

        public string BasePriceListId { get; set; }

        public string PriorityPriceListId { get; set; }

        public List<string> ProductIds { get; set; }
    }
}
