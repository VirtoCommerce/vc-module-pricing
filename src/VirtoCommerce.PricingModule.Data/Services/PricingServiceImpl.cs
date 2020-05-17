using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingServiceImpl : IPricingService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IItemService _productService;
        private readonly ILogger<PricingServiceImpl> _logger;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;

        public PricingServiceImpl(
            Func<IPricingRepository> repositoryFactory
            , IItemService productService
            , ILogger<PricingServiceImpl> logger
            , IPlatformMemoryCache platformMemoryCache
            , IEventPublisher eventPublisher
            , IPricingPriorityFilterPolicy pricingPriorityFilterPolicy)
        {
            _repositoryFactory = repositoryFactory;
            _productService = productService;
            _logger = logger;
            _platformMemoryCache = platformMemoryCache;
            _eventPublisher = eventPublisher;
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
        }

        #region IPricingService Members
        /// <summary>
        /// Evaluate pricelists for special context. All resulting pricelists ordered by priority
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(EvaluatePriceListsAsync));
            var priceListAssignments = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(PricingCacheRegion.CreateChangeToken());

                return await GetAllPricelistAssignments();
            });

            var query = priceListAssignments.AsQueryable();

            if (evalContext.CatalogId != null)
            {
                //filter by catalog
                query = query.Where(x => x.CatalogId == evalContext.CatalogId);
            }

            if (evalContext.Currency != null)
            {
                //filter by currency
                query = query.Where(x => x.Pricelist.Currency == evalContext.Currency.ToString());
            }

            if (evalContext.CertainDate != null)
            {
                //filter by date expiration
                query = query.Where(x => (x.StartDate == null || evalContext.CertainDate >= x.StartDate) && (x.EndDate == null || x.EndDate >= evalContext.CertainDate));
            }

            var assignments = query.ToArray();
            var assignmentsToReturn = assignments.Where(x => x.DynamicExpression == null).ToList();

            foreach (var assignment in assignments.Where(x => x.DynamicExpression != null))
            {
                try
                {
                    if (assignment.DynamicExpression.IsSatisfiedBy(evalContext) && assignmentsToReturn.All(x => x.PricelistId != assignment.PricelistId))
                    {
                        assignmentsToReturn.Add(assignment);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to evaluate price assignment condition.");
                }
            }

            return assignmentsToReturn.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Name).Select(x => x.Pricelist);
        }

        public virtual async Task<PricelistAssignment[]> GetAllPricelistAssignments()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                return (await repository.PricelistAssignments.Include(x => x.Pricelist).ToArrayAsync()).Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
            }
        }
        /// <summary>
        /// Evaluation product prices.
        /// Will get either all prices or one price per currency depending on the settings in evalContext.
        /// </summary>
        /// <param name="evalContext"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
        {
            if (evalContext == null)
            {
                throw new ArgumentNullException(nameof(evalContext));
            }
            if (evalContext.ProductIds == null)
            {
                throw new MissingFieldException("ProductIds");
            }

            var retVal = new List<Price>();
            Price[] prices;
            using (var repository = _repositoryFactory())
            {
                //Get a price range satisfying by passing context
                var query = repository.Prices.Include(x => x.Pricelist)
                                             .Where(x => evalContext.ProductIds.Contains(x.ProductId))
                                             .Where(x => evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0);

                if (evalContext.PricelistIds.IsNullOrEmpty())
                {
                    evalContext.PricelistIds = (await EvaluatePriceListsAsync(evalContext)).Select(x => x.Id).ToArray();
                }

                query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));

                // Filter by date expiration
                // Always filter on date, so that we limit the results to process.
                var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;
                query = query.Where(x => (x.StartDate == null || x.StartDate <= certainDate)
                    && (x.EndDate == null || x.EndDate > certainDate));

                var queryResult = await query.ToArrayAsync();
                prices = queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance())).ToArray();
            }

            //Apply pricing  filtration strategy for found prices
            retVal.AddRange(_pricingPriorityFilterPolicy.FilterPrices(prices, evalContext));

            //Then variation inherited prices
            if (_productService != null)
            {
                var productIdsWithoutPrice = evalContext.ProductIds.Except(retVal.Select(x => x.ProductId).Distinct()).ToArray();
                //Variation price inheritance
                //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
                if (productIdsWithoutPrice.Any())
                {
                    var variations = (await _productService.GetByIdsAsync(productIdsWithoutPrice, ItemResponseGroup.ItemInfo.ToString())).Where(x => x.MainProductId != null).ToList();
                    evalContext.ProductIds = variations.Select(x => x.MainProductId).Distinct().ToArray();

                    var inheritedPrices = await EvaluateProductPricesAsync(evalContext);
                    foreach (var inheritedPrice in inheritedPrices)
                    {
                        foreach (var variation in variations.Where(x => x.MainProductId == inheritedPrice.ProductId))
                        {
                            var jObject = JObject.FromObject(inheritedPrice);
                            var variationPrice = (Price)jObject.ToObject(inheritedPrice.GetType());
                            //For correct override price in possible update 
                            variationPrice.Id = null;
                            variationPrice.ProductId = variation.Id;
                            retVal.Add(variationPrice);
                        }
                    }
                }
            }

            return retVal;
        }


        public virtual async Task<Price[]> GetPricesByIdAsync(string[] ids)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetPricesByIdAsync), string.Join("-", ids));
            var result = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                Price[] result = null;
                if (ids != null)
                {
                    using (var repository = _repositoryFactory())
                    {
                        cacheEntry.AddExpirationToken(PricesCacheRegion.CreateChangeToken(ids));

                        result = (await repository.GetPricesByIdsAsync(ids)).Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance())).ToArray();
                    }
                }

                return result;
            });

            return result.Select(x => x.Clone() as Price).ToArray();
        }

        public virtual async Task<PricelistAssignment[]> GetPricelistAssignmentsByIdAsync(string[] ids)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetPricelistAssignmentsByIdAsync), string.Join("-", ids));
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                PricelistAssignment[] result = null;
                if (ids != null)
                {
                    using (var repository = _repositoryFactory())
                    {
                        cacheEntry.AddExpirationToken(PricelistAssignmentsCacheRegion.CreateChangeToken(ids));
                        result = (await repository.GetPricelistAssignmentsByIdAsync(ids)).Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
                    }
                }
                return result;
            });
        }

        public virtual async Task<Pricelist[]> GetPricelistsByIdAsync(string[] ids)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetPricelistsByIdAsync), string.Join("-", ids));
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                Pricelist[] result = null;
                if (ids != null)
                {
                    using (var repository = _repositoryFactory())
                    {
                        cacheEntry.AddExpirationToken(PricelistsCacheRegion.CreateChangeToken(ids));

                        var pricelistEntities = await repository.GetPricelistByIdsAsync(ids);
                        result = pricelistEntities.Select(x => x.ToModel(AbstractTypeFactory<Pricelist>.TryCreateInstance())).ToArray();
                    }
                }

                return result;
            });
        }

        public virtual async Task SavePricesAsync(Price[] prices)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Price>>();
            using (var repository = _repositoryFactory())
            {
                var pricesIds = prices.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistPricesEntities = await repository.GetPricesByIdsAsync(pricesIds);

                //Create default priceLists for prices without pricelist 
                foreach (var priceWithoutPricelistGroup in prices.Where(x => x.PricelistId == null).GroupBy(x => x.Currency))
                {
                    var defaultPriceListId = GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    var pricelists = await GetPricelistsByIdAsync(new[] { defaultPriceListId });
                    if (pricelists.IsNullOrEmpty())
                    {
                        var defaultPriceList = AbstractTypeFactory<Pricelist>.TryCreateInstance();
                        defaultPriceList.Id = defaultPriceListId;
                        defaultPriceList.Currency = priceWithoutPricelistGroup.Key;
                        defaultPriceList.Name = defaultPriceListId;
                        defaultPriceList.Description = defaultPriceListId;
                        repository.Add(AbstractTypeFactory<PricelistEntity>.TryCreateInstance().FromModel(defaultPriceList, pkMap));
                    }
                    foreach (var priceWithoutPricelist in priceWithoutPricelistGroup)
                    {
                        priceWithoutPricelist.PricelistId = defaultPriceListId;
                    }
                }

                foreach (var price in prices)
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

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();
                await _eventPublisher.Publish(new PriceChangedEvent(changedEntries));

                foreach (var price in prices)
                {
                    PricesCacheRegion.ExpirePrice(price.Id);
                }
                ResetCache();
            }
        }

        public virtual async Task SavePricelistsAsync(Pricelist[] priceLists)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                var pricelistsIds = priceLists.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = await repository.GetPricelistByIdsAsync(pricelistsIds);

                foreach (var pricelist in priceLists)
                {
                    var sourceEntity = AbstractTypeFactory<PricelistEntity>.TryCreateInstance().FromModel(pricelist, pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == pricelist.Id);
                    if (targetEntity != null)
                    {
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                foreach (var pricelist in priceLists)
                {
                    PricelistsCacheRegion.ExpirePricelist(pricelist.Id);
                }
                ResetCache();
            }
        }

        public virtual async Task SavePricelistAssignmentsAsync(PricelistAssignment[] assignments)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            using (var repository = _repositoryFactory())
            {
                var assignmentsIds = assignments.Select(x => x.Id).Where(x => x != null).Distinct().ToArray();
                var alreadyExistEntities = await repository.GetPricelistAssignmentsByIdAsync(assignmentsIds);

                foreach (var assignment in assignments)
                {
                    var sourceEntity = AbstractTypeFactory<PricelistAssignmentEntity>.TryCreateInstance().FromModel(assignment, pkMap);
                    var targetEntity = alreadyExistEntities.FirstOrDefault(x => x.Id == assignment.Id);
                    if (targetEntity != null)
                    {
                        sourceEntity.Patch(targetEntity);
                    }
                    else
                    {
                        repository.Add(sourceEntity);
                    }
                }

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                foreach (var assignment in assignments)
                {
                    PricelistAssignmentsCacheRegion.ExpirePricelistAssignment(assignment.Id);
                }
                ResetCache();
            }
        }

        public virtual async Task DeletePricesAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                var prices = await GetPricesByIdAsync(ids);
                var changedEntries = prices.Select(x => new GenericChangedEntry<Price>(x, EntryState.Deleted));
                await repository.DeletePricesAsync(ids);
                await repository.UnitOfWork.CommitAsync();
                await _eventPublisher.Publish(new PriceChangedEvent(changedEntries));

                foreach (var id in ids)
                {
                    PricesCacheRegion.ExpirePrice(id);
                }
                ResetCache();
            }
        }
        public virtual async Task DeletePricelistsAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.DeletePricelistsAsync(ids);
                await repository.UnitOfWork.CommitAsync();

                foreach (var id in ids)
                {
                    PricelistsCacheRegion.ExpirePricelist(id);
                }
                ResetCache();
            }
        }

        public virtual async Task DeletePricelistsAssignmentsAsync(string[] ids)
        {
            using (var repository = _repositoryFactory())
            {
                await repository.DeletePricelistAssignmentsAsync(ids);
                await repository.UnitOfWork.CommitAsync();

                foreach (var id in ids)
                {
                    PricelistAssignmentsCacheRegion.ExpirePricelistAssignment(id);
                }
                ResetCache();
            }
        }
        #endregion

        private static string GetDefaultPriceListName(string currency)
        {
            var retVal = "Default" + currency;
            return retVal;
        }

        private void ResetCache()
        {
            //Clear cache (Smart cache implementation) 
            PricingCacheRegion.ExpireRegion();
            PricingSearchCacheRegion.ExpireRegion();
        }
              
    }
}
