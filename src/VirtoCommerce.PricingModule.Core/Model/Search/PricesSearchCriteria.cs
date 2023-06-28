using System;
using System.Collections.Generic;

namespace VirtoCommerce.PricingModule.Core.Model.Search
{
    public class PricesSearchCriteria : PricingSearchCriteria
    {
        //Apply paginate limits to product count instead prices record
        public bool GroupByProducts { get; set; }

        public string PriceListId { get; set; }

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
            set
            {
                _priceListIds = value;
            }
        }

        public string ProductId { get; set; }

        private IList<string> _productIds;
        public IList<string> ProductIds
        {
            get
            {
                if (_productIds == null && !string.IsNullOrEmpty(ProductId))
                {
                    _productIds = new[] { ProductId };
                }
                return _productIds;
            }
            set
            {
                _productIds = value;
            }
        }

        public DateTime? ModifiedSince { get; set; }
    }
}
