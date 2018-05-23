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

            foreach (var changedEntry in message.ChangedEntries.ToList())
            {
                DeletePrices(changedEntry);
            }

            return Task.CompletedTask;
        }

        protected virtual void DeletePrices(GenericChangedEntry<CatalogProduct> changedEntry)
        {
            if (changedEntry.EntryState == EntryState.Deleted)
            {
                var product = changedEntry.OldEntry;

                var searchResult = _pricingSearchService.SearchPrices(new PricesSearchCriteria { ProductId = product.Id, Take = int.MaxValue});

                _pricingService.DeletePrices(searchResult.Results.Select(p => p.Id).ToArray());
            }
        }
    }
}
