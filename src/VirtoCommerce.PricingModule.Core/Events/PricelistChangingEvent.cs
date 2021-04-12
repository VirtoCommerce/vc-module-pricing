using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Events
{
    public class PricelistChangingEvent : GenericChangedEntryEvent<Pricelist>
    {
        public PricelistChangingEvent(IEnumerable<GenericChangedEntry<Pricelist>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
