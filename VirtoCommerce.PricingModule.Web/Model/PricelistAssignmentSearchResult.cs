using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtoCommerce.PricingModule.Web.Model
{
    public class PricelistAssignmentSearchResult
    {
        public long TotalCount { get; set; }
        public ICollection<PricelistAssignment> Assignments { get; set; }
    }
}