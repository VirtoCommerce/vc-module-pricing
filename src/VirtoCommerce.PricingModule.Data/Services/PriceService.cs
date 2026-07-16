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
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PriceService : OuterEntityService<Price, PriceEntity, PriceChangingEvent, PriceChangedEvent>, IPriceService
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

                // AC-12 completeness: an edit that moves a price to a different (PricelistId, ProductId)
                // leaves the OLD key's evaluator-cache entry unexpired if we only look at ClearCache's
                // post-edit models. changedEntries still holds the pre-edit OldEntry here, so expire the
                // old key too. No-op when the key is unchanged.
                foreach (var changedEntry in changedEntries.Where(x => x.EntryState == EntryState.Modified))
                {
                    var oldPricelistId = changedEntry.OldEntry.PricelistId;
                    var oldProductId = changedEntry.OldEntry.ProductId;
                    if (oldPricelistId != null && oldProductId != null
                        && (oldPricelistId != changedEntry.NewEntry.PricelistId || oldProductId != changedEntry.NewEntry.ProductId))
                    {
                        GenericCachingRegion<Price>.ExpireTokenForKey(PriceEvaluationCacheKey.TokenKey(oldPricelistId, oldProductId));
                    }
                }

                await _eventPublisher.Publish(new PriceChangedEvent(changedEntries));
            }
        }

        protected override Task<IList<PriceEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IPricingRepository)repository).GetPricesByIdsAsync(ids);
        }

        protected override IQueryable<PriceEntity> GetEntitiesQuery(IRepository repository)
        {
            return ((IPricingRepository)repository).Prices;
        }

        protected override void ClearCache(IList<Price> models)
        {
            // AC-12: invalidate ONLY the changed (pricelistId, productId) evaluator entries.
            // ExpireRegion() must NOT be fired here: CreateChangeTokenForKey composites include the
            // region token (CancellableCacheRegion.cs:105), so a region flush would drop every product's
            // entry and defeat per-key precision. Search caches stay invalidated via base.ClearCache
            // (GenericSearchCachingRegion<Price>), which this override still calls.
            foreach (var price in models.Where(x => x.PricelistId != null && x.ProductId != null))
            {
                GenericCachingRegion<Price>.ExpireTokenForKey(PriceEvaluationCacheKey.TokenKey(price.PricelistId, price.ProductId));
            }

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

        private static string GetDefaultPriceListName(string currency)
        {
            var result = "Default" + currency;
            return result;
        }
    }
}
