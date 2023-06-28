using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Data.Search;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class IndexPricesProductChangedEventHandler : IEventHandler<PriceChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexPricesProductChangedEventHandler(
            ISettingsManager settingsManager,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _indexingJobService = indexingJobService;
            _configurations = configurations;
        }

        public async Task Handle(PriceChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.EventBasedIndexation))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.ProductId, EntryState = EntryState.Modified, Type = KnownDocumentTypes.Product })
                    .ToArray();

                _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                    _configurations.GetDocumentBuilders(KnownDocumentTypes.Product, typeof(ProductPriceDocumentChangesProvider)).ToList());
            }
        }
    }
}
