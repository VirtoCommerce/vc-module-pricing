using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Data.Common;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class ObjectSettingEntryChangedEventHandler : IEventHandler<Platform.Core.Settings.Events.ObjectSettingChangedEvent>
    {
        private readonly ModuleConfigurator _moduleConfigurator;


        public ObjectSettingEntryChangedEventHandler(ModuleConfigurator moduleConfigurator)
        {
            _moduleConfigurator = moduleConfigurator;

        }

        public virtual async Task Handle(Platform.Core.Settings.Events.ObjectSettingChangedEvent message)
        {
            if (message.ChangedEntries.Any(x => (x.EntryState == EntryState.Modified
                                                || x.EntryState == EntryState.Added)
                                           && x.NewEntry.Name == Core.ModuleConstants.Settings.General.PricingIndexing.Name))
            {
                await _moduleConfigurator.ConfigureSearchAsync();
            }
        }
    }
}
