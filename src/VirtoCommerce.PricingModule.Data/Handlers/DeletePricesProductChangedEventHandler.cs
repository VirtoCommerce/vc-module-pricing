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
        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;

        public DeletePricesProductChangedEventHandler(IPricingService pricingService, IPricingSearchService pricingSearchService)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
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
                var searchResult = await _pricingSearchService.SearchPricesAsync(new PricesSearchCriteria { ProductIds = deletedProductIds, Take = int.MaxValue });
                if (searchResult.Results.Any())
                {
                    await _pricingService.DeletePricesAsync(searchResult.Results.Select(p => p.Id).ToArray());
                }
            }
        }
    }
}
