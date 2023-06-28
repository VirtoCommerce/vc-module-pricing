using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Caching;
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
    public class PriceService : CrudService<Price, PriceEntity, PriceChangingEvent, PriceChangedEvent>, IPriceService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPricelistService _pricelistService;

        public PriceService(
            Func<IPricingRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            IPricelistService pricelistService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _pricelistService = pricelistService;
        }


        public override async Task SaveChangesAsync(IList<Price> models)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Price>>();
            using (var repository = _repositoryFactory())
            {
                var alreadyExistPricesEntities = await LoadEntities(repository, models.Select(x => x.Id).Where(x => x != null).Distinct().ToList());

                //Create default priceLists for prices without pricelist 
                foreach (var priceWithoutPricelistGroup in models.Where(x => x.PricelistId == null).GroupBy(x => x.Currency))
                {
                    var defaultPriceListId = GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    var pricelist = await _pricelistService.GetNoCloneAsync(defaultPriceListId);
                    if (pricelist == null)
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
                    var sourceEntity = AbstractTypeFactory<PriceEntity>.TryCreateInstance().FromModel(price, pkMap);
                    var targetEntity = alreadyExistPricesEntities.FirstOrDefault(x => x.Id == price.Id);
                    if (targetEntity != null)
                    {
                        changedEntries.Add(new GenericChangedEntry<Price>(price, targetEntity.ToModel(AbstractTypeFactory<Price>.TryCreateInstance()), EntryState.Modified));
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        changedEntries.Add(new GenericChangedEntry<Price>(price, EntryState.Added));
                        repository.Add(sourceEntity);
                    }
                }

                await _eventPublisher.Publish(new PriceChangingEvent(changedEntries));

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                ClearCache(models);

                await _eventPublisher.Publish(new PriceChangedEvent(changedEntries));
            }
        }

        protected override Task<IList<PriceEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IPricingRepository)repository).GetPricesByIdsAsync(ids);
        }

        protected override void ClearCache(IList<Price> models)
        {
            GenericCachingRegion<Price>.ExpireRegion();
            base.ClearCache(models);
        }

        protected virtual Pricelist GetDefaultPriceList(IGrouping<string, Price> priceWithoutPricelistGroup, string defaultPriceListId)
        {
            var defaultPriceList = AbstractTypeFactory<Pricelist>.TryCreateInstance();
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
