using System;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model.Conditions;

namespace VirtoCommerce.PricingModule.Core.Model
{
    /// <summary>
    /// Used to assign pricelist to specific catalog by using conditional expression 
    /// </summary>
	public class PricelistAssignment : AuditableEntity, IHasOuterId, ICloneable
    {
        public string CatalogId { get; set; }
        public string StoreId { get; set; }
        public string PricelistId { get; set; }
        public Pricelist Pricelist { get; set; }
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
        public PriceConditionTree DynamicExpression { get; set; }

        public string OuterId { get; set; }


        #region ICloneable members
        public virtual object Clone()
        {
            var result = (PricelistAssignment)MemberwiseClone();

            result.Pricelist = Pricelist?.CloneTyped();
            result.DynamicExpression = DynamicExpression?.CloneTyped();

            return result;
        }
        #endregion
    }
}
