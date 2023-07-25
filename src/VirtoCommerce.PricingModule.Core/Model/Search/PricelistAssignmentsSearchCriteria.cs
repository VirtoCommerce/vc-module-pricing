using System.Collections.Generic;

namespace VirtoCommerce.PricingModule.Core.Model.Search
{
    public class PricelistAssignmentsSearchCriteria : PricingSearchCriteria
    {
        public string PriceListId { get; set; }
        public IList<string> CatalogIds { get; set; }
        public IList<string> StoreIds { get; set; }

        private IList<string> _priceListIds;
        public IList<string> PriceListIds
        {
            get
            {
                if (_priceListIds == null && !string.IsNullOrEmpty(PriceListId))
                {
                    _priceListIds = new[] { PriceListId };
                }
                return _priceListIds;
            }
            set => _priceListIds = value;
        }

    }
}
