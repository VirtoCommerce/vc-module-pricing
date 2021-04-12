using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Events
{
    public class PricelistAssignmentChangingEvent : GenericChangedEntryEvent<PricelistAssignment>
    {
        public PricelistAssignmentChangingEvent(IEnumerable<GenericChangedEntry<PricelistAssignment>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
