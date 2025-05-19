using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PricingModule.Core.Model
{
    public class Pricelist : AuditableEntity, IHasOuterId, ICloneable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Currency { get; set; }
        public string OuterId { get; set; }
        public int Priority { get; set; }
        public ICollection<Price> Prices { get; set; }
        public ICollection<PricelistAssignment> Assignments { get; set; }

        #region ICloneable members
        public virtual object Clone()
        {
            var result = (Pricelist)MemberwiseClone();

            result.Prices = Prices?.Select(x => x.CloneTyped()).ToList();
            result.Assignments = Assignments?.Select(x => x.CloneTyped()).ToList();

            return result;
        }
        #endregion
    }
}
