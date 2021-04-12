using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Events
{
    public class PricelistChangedEvent : GenericChangedEntryEvent<Pricelist>
    {
        public PricelistChangedEvent(IEnumerable<GenericChangedEntry<Pricelist>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
