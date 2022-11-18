using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services.Crud.Basic
{
    public class BasicPriceService<TModel, TEntity, TChangeEvent, TChangedEvent, TPriceListModel> : CrudService<TModel, TEntity, TChangeEvent, TChangedEvent>
        where TModel : Price
        where TEntity : PriceEntity, Platform.Core.Domain.IDataEntity<TEntity, TModel>
        where TChangeEvent : GenericChangedEntryEvent<TModel>
        where TChangedEvent : GenericChangedEntryEvent<TModel>
        where TPriceListModel : Pricelist
    {
        private readonly ICrudService<TPriceListModel> _pricelistService;
        public BasicPriceService(Func<IPricingRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher, ICrudService<TPriceListModel> pricelistService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _pricelistService = pricelistService;
        }

        public override async Task SaveChangesAsync(IEnumerable<TModel> models)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<TModel>>();
            using (var repository = _repositoryFactory())
            {
                var alreadyExistPricesEntities = await LoadEntities(repository, models.Select(x => x.Id).Where(x => x != null).Distinct().ToList());

                //Create default priceLists for prices without pricelist 
                foreach (var priceWithoutPricelistGroup in models.Where(x => x.PricelistId == null).GroupBy(x => x.Currency))
                {
                    var defaultPriceListId = GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    var pricelists = await _pricelistService.GetAsync(new List<string> { defaultPriceListId });
                    if (pricelists.IsNullOrEmpty())
                    {
                        repository.Add(AbstractTypeFactory<PricelistEntity>.TryCreateInstance().FromModel(GetDefaultPriceList(priceWithoutPricelistGroup, defaultPriceListId), pkMap));
                    }
                    foreach (var priceWithoutPricelist in priceWithoutPricelistGroup)
                    {
                        priceWithoutPricelist.PricelistId = defaultPriceListId;
                    }
                }

                foreach (var price in models)
                {
                    var sourceEntity = AbstractTypeFactory<TEntity>.TryCreateInstance().FromModel(price, pkMap);
                    var targetEntity = alreadyExistPricesEntities.FirstOrDefault(x => x.Id == price.Id);
                    if (targetEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<TModel>(price, (TModel)targetEntity.ToModel(AbstractTypeFactory<TModel>.TryCreateInstance()), EntryState.Modified));
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        changedEntries.Add(new GenericChangedEntry<TModel>(price, EntryState.Added));
                        repository.Add(sourceEntity);
                    }
                }

                await _eventPublisher.Publish(EventFactory<TChangeEvent>(changedEntries));

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                ClearCache(models);

                await _eventPublisher.Publish(EventFactory<TChangedEvent>(changedEntries));
            }
        }

        protected override async Task<IEnumerable<TEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return (IEnumerable<TEntity>)await ((IPricingRepository)repository).GetPricesByIdsAsync(ids);
        }

        protected override void ClearCache(IEnumerable<TModel> models)
        {
            GenericCachingRegion<TModel>.ExpireRegion();
            base.ClearCache(models);
        }

        protected virtual Pricelist GetDefaultPriceList(IGrouping<string, TModel> priceWithoutPricelistGroup, string defaultPriceListId)
        {
            var defaultPriceList = AbstractTypeFactory<TPriceListModel>.TryCreateInstance();
            defaultPriceList.Id = defaultPriceListId;
            defaultPriceList.Currency = priceWithoutPricelistGroup.Key;
            defaultPriceList.Name = defaultPriceListId;
            defaultPriceList.Description = defaultPriceListId;
            return defaultPriceList;
        }
        private string GetDefaultPriceListName(string currency)
        {
            var result = "Default" + currency;
            return result;
        }

    }
}
