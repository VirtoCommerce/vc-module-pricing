using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class DeletePricesProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private const int _batchSize = 100;

        private readonly IPriceService _priceService;
        private readonly IPriceSearchService _priceSearchService;

        public DeletePricesProductChangedEventHandler(IPriceService priceService, IPriceSearchService priceSearchService)
        {
            _priceService = priceService;
            _priceSearchService = priceSearchService;
        }

        public virtual async Task Handle(ProductChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var deletedProductIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();
            if (!deletedProductIds.IsNullOrEmpty())
            {
                PriceSearchResult searchResult;
                do
                {
                    searchResult = await _priceSearchService.SearchNoCloneAsync(
                        new PricesSearchCriteria
                        {
                            ProductIds = deletedProductIds,
                            Take = _batchSize
                        });

                    if (searchResult.Results.Any())
                    {
                        await _priceService.DeleteAsync(searchResult.Results.Select(p => p.Id).ToArray());
                    }
                }
                while (searchResult.TotalCount > 0);
            }
        }
    }
}
