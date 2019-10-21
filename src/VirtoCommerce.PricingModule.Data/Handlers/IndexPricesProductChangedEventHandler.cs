using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class IndexPricesProductChangedEventHandler : IEventHandler<PriceChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;
        private static readonly EntryState[] _entityStates = new[] { EntryState.Added, EntryState.Modified, EntryState.Deleted };

        public IndexPricesProductChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public Task Handle(PriceChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexProductIds = message.ChangedEntries.Where(x => _entityStates.Any(s => s == x.EntryState)
                                                                    && x.OldEntry.ProductId != null)
                                                          .Select(x => x.OldEntry.ProductId)
                                                          .Distinct().ToArray();

            if (!indexProductIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexPricesProductBackgroundJob(indexProductIds));
            }

            return Task.CompletedTask;
        }


        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexPricesProductBackgroundJob(string[] indexProductIds)
        {
            await TryIndexPricesProduct(indexProductIds);
        }


        protected virtual async Task TryIndexPricesProduct(string[] indexProductIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexProductIds);
        }
    }
}
