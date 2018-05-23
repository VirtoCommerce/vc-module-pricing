using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Catalog.Events;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.PricingModule.Data.Handlers
{
    public class DeletePricesProductChangedEvent : IEventHandler<ProductChangedEvent>
    {

        private readonly IPricingService _pricingService;

        public DeletePricesProductChangedEvent(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        public virtual Task Handle(ProductChangedEvent message)
        {

            foreach (var changedEntry in message.ChangedEntries.ToList())
            {
                DeletePrices(changedEntry);
            }

            return Task.CompletedTask;
        }

        private void DeletePrices(GenericChangedEntry<CatalogProduct> changedEntry)
        {
            if (changedEntry.EntryState == EntryState.Deleted)
            {
                var product = changedEntry.OldEntry;

                _pricingService.DeletePrices(product.Prices.Select(p => p.Id).ToArray());
            }
        }
    }
}
