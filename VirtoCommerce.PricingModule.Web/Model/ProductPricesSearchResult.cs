using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.PricingModule.Web.Model
{
    public class ProductPricesSearchResult
    {
        public long TotalCount { get; set; }
        public ICollection<ProductPrice> ProductPrices { get; set; }
    }
}