using System;
using VirtoCommerce.CoreModule.Core.Common;

namespace VirtoCommerce.PricingModule.Core.Model
{
	public class PriceEvaluationContext : EvaluationContextBase, ICloneable
	{
		public string StoreId { get; set; } 
		public string CatalogId { get; set; }
		public string[] ProductIds { get; set; }
        // The list of price-lists, evaluation logic will return only matched product prices from the price-list with hightest priority
        // To return all the prices found, simply set ReturnAllMatchedPrices to true
		public Pricelist[] Pricelists { get; set; }
        // Set this flag to true for return all matched prices from all given pricelists 
	    public bool ReturnAllMatchedPrices { get; set; }
        public decimal Quantity { get; set; }
		public string CustomerId { get; set; }
		public string OrganizationId { get; set; }
		public DateTime? CertainDate { get; set; }
		public string Currency { get; set; }
        // Set this flag to true for indexing from all given pricelists and skip Dynamic Conditions except Start and End Date  
        public bool SkipAssignmentValidation { get; set; }

        public object Clone()
        {
            return base.MemberwiseClone() as PriceEvaluationContext;
        }
    }
}
