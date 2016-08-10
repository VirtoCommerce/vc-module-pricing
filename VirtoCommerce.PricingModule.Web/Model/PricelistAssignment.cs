using System;
using VirtoCommerce.CatalogModule.Web.Model;
using VirtoCommerce.Domain.Common;
using VirtoCommerce.Platform.Core.Common;
namespace VirtoCommerce.PricingModule.Web.Model
{
	public class PricelistAssignment : AuditableEntity
	{
		public string CatalogId { get; set; }
        public Catalog Catalog { get; set; }
		public string PricelistId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

        /// <summary>
        /// If two PricelistAssignments satisfies the conditions and rules, will use one with the greater priority
		/// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Start of period when Prices Assignment is valid. Null value means no limit
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End of period when Prices Assignment is valid. Null value means no limit
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// List of conditions and rules to define Prices Assignment is valid
        /// </summary>
        public ConditionExpressionTree DynamicExpression { get; set; }
	}
}