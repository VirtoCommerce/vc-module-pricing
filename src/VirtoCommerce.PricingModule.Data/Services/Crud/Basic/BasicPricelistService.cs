using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services.Crud.Basic
{
    public class BasicPricelistService<TModel, TEntity, TChangeEvent, TChangedEvent> : CrudService<TModel, TEntity, TChangeEvent, TChangedEvent>
        where TModel : Pricelist
        where TEntity : PricelistEntity, Platform.Core.Domain.IDataEntity<TEntity, TModel>
        where TChangeEvent : GenericChangedEntryEvent<TModel>
        where TChangedEvent : GenericChangedEntryEvent<TModel>
    {
        public BasicPricelistService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override async Task<IEnumerable<TEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return (IEnumerable<TEntity>)await ((IPricingRepository)repository).GetPricelistByIdsAsync(ids, responseGroup);
        }
    }
}
