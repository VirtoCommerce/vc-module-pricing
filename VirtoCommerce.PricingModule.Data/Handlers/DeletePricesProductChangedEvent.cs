using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class DeletePricesProductChangedEvent : IEventHandler<ProductChangedEvent>
    {

        private readonly IPricingService _pricingService;
        private readonly IPricingSearchService _pricingSearchService;

        public DeletePricesProductChangedEvent(IPricingService pricingService, IPricingSearchService pricingSearchService)
        {
            _pricingService = pricingService;
            _pricingSearchService = pricingSearchService;
        }

        public virtual Task Handle(ProductChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var deletedProductIds = message.ChangedEntries.Where(x => x.EntryState == EntryState.Deleted && x.OldEntry.Id != null)
                                                          .Select(x => x.OldEntry.Id)
                                                          .Distinct().ToArray();
            var searchResult = _pricingSearchService.SearchPrices(new PricesSearchCriteria { ProductIds = deletedProductIds, Take = int.MaxValue });
            if (searchResult.Results.Any())
            {
                _pricingService.DeletePrices(searchResult.Results.Select(p => p.Id).ToArray());
            }

            return Task.CompletedTask;
        }
    }
}
