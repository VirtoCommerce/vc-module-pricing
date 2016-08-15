using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.PricingModule.Web.Model
{
    public class PricelistSearchResult
    {
        public long TotalCount { get; set; }
        public ICollection<Pricelist> Pricelists { get; set; }
    }
}