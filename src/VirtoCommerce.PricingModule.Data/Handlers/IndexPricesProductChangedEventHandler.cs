using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    using VirtoCommerce.PricingModule.Core.Events;
    using VirtoCommerce.SearchModule.Core.Model;
    using VirtoCommerce.SearchModule.Core.Services;

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
        public Task TryIndexPricesProductBackgroundJob(string[] indexProductIds)
        {
            return TryIndexPricesProduct(indexProductIds);
        }

        protected virtual Task TryIndexPricesProduct(string[] indexProductIds)
        {
            return _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexProductIds);
        }
    }
}
