using System;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;
using VirtoCommerce.PricingModule.Data.Services.Crud.Basic;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PriceService : BasicPriceService<Price, PriceEntity, PriceChangingEvent, PriceChangedEvent, Pricelist>
    {
        public PriceService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher, ICrudService<Pricelist> pricelistService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher, pricelistService)
        {
        }
    }
}
