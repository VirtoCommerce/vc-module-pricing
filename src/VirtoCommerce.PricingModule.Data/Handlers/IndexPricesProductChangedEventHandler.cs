using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;
using VirtoCommerce.SearchModule.Data.Services;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class IndexPricesProductChangedEventHandler : IEventHandler<PriceChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexPricesProductChangedEventHandler(ISettingsManager settingsManager, IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _configurations = configurations;
        }

        public Task Handle(PriceChangedEvent message)
        {
            if (_settingsManager.GetValue(Core.ModuleConstants.Settings.General.EventBasedIndexation.Name, false))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.ProductId, EntryState = EntryState.Modified, Type = KnownDocumentTypes.Product })
                    .ToArray();

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries,
                    JobPriority.Normal, _configurations.GetBuildersForProvider(typeof(ProductPriceDocumentChangesProvider)).ToList());
            }

            return Task.CompletedTask;
        }
    }
}
