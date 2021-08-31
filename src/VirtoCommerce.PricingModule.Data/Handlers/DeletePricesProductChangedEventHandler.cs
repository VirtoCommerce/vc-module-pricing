using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class DeletePricesProductChangedEventHandler : IEventHandler<ProductChangedEvent>
    {
        private readonly ICrudService<Price> _priceService;
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;

        public DeletePricesProductChangedEventHandler(IPriceService priceService, IPriceSearchService priceSearchService)
        {
            _priceService = (ICrudService<Price>)priceService;
            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
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
                var searchResult = await _priceSearchService.SearchAsync(new PricesSearchCriteria { ProductIds = deletedProductIds, Take = int.MaxValue });
                if (searchResult.Results.Any())
                {
                    await _priceService.DeleteAsync(searchResult.Results.Select(p => p.Id).ToArray());
                }
            }
        }
    }
}
