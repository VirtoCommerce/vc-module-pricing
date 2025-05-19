using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistService : OuterEntityService<Pricelist, PricelistEntity, PricelistChangingEvent, PricelistChangedEvent>, IPricelistService
    {
        public PricelistService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task<IList<PricelistEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IPricingRepository)repository).GetPricelistByIdsAsync(ids, responseGroup);
        }

        protected override IQueryable<PricelistEntity> GetEntitiesQuery(IRepository repository)
        {
            return ((IPricingRepository)repository).Pricelists;
        }
    }
}
