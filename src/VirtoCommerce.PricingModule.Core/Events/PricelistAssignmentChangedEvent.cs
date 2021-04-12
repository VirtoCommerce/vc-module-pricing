using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Events
{
    public class PricelistAssignmentChangedEvent : GenericChangedEntryEvent<PricelistAssignment>
    {
        public PricelistAssignmentChangedEvent(IEnumerable<GenericChangedEntry<PricelistAssignment>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
