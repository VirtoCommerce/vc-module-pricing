using System.Collections.Generic;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Services
{
    /// <summary>
    /// Represents a abstraction for filtering  prices based on business rules.
    /// </summary>
    public interface IPricingPriorityFilterPolicy
    {
        IEnumerable<Price> FilterPrices(IEnumerable<Price> prices, PriceEvaluationContext evalContext);
    }
}
