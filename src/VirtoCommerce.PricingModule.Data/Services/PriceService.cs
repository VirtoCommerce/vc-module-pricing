using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
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
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;
        private readonly IPricelistAssignmentService _pricelistAssignmentService;
        private readonly IPricelistService _pricelistService;
        private readonly IItemService _productService;

        public PriceService(Func<IPriceRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher
            , IPricingPriorityFilterPolicy pricingPriorityFilterPolicy, IPricelistAssignmentService pricelistAssignmentService
            , IPricelistService pricelistService, IItemService productService)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
            _pricelistAssignmentService = pricelistAssignmentService;
            _productService = productService;
            _pricelistService = pricelistService;
        }


        public override async Task SaveChangesAsync(IEnumerable<Price> prices)
        {
            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Price>>();
            using (var repository = _repositoryFactory())
            {
                var alreadyExistPricesEntities = await LoadEntities(repository, prices.Select(x => x.Id).Where(x => x != null).Distinct().ToArray());

                //Create default priceLists for prices without pricelist 
                foreach (var priceWithoutPricelistGroup in prices.Where(x => x.PricelistId == null).GroupBy(x => x.Currency))
                {
                    var defaultPriceListId = _pricelistService.GetDefaultPriceListName(priceWithoutPricelistGroup.Key);
                    var pricelists = await ((ICrudService<Pricelist>)_pricelistService).GetByIdsAsync(new[] { defaultPriceListId });
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

                await _eventPublisher.Publish(new PriceChangingEvent(changedEntries));

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();

                ClearCache(prices);

                await _eventPublisher.Publish(new PriceChangedEvent(changedEntries));
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

            var result = new List<Price>();
            Price[] prices;
            using (var repository = _repositoryFactory())
            {
                //Get a price range satisfying by passing context
                var query = ((IPriceRepository)repository).Prices.Include(x => x.Pricelist)
                                             .Where(x => evalContext.ProductIds.Contains(x.ProductId))
                                             .Where(x => evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0);

                if (evalContext.PricelistIds.IsNullOrEmpty())
                {
                    evalContext.PricelistIds = (await _pricelistAssignmentService.EvaluatePriceListsAsync(evalContext)).Select(x => x.Id).ToArray();
                }

                query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));

                // Filter by date expiration
                // Always filter on date, so that we limit the results to process.
                var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;
                query = query.Where(x => (x.StartDate == null || x.StartDate <= certainDate)
                    && (x.EndDate == null || x.EndDate > certainDate));

                var queryResult = await query.AsNoTracking().ToArrayAsync();
                prices = queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance())).ToArray();
            }

            //Apply pricing  filtration strategy for found prices
            result.AddRange(_pricingPriorityFilterPolicy.FilterPrices(prices, evalContext));

            //Then variation inherited prices
            if (_productService != null)
            {
                var productIdsWithoutPrice = evalContext.ProductIds.Except(result.Select(x => x.ProductId).Distinct()).ToArray();
                //Try to inherit prices for variations from their main product
                //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
                if (productIdsWithoutPrice.Any())
                {
                    var variations = (await _productService.GetByIdsAsync(productIdsWithoutPrice, ItemResponseGroup.ItemInfo.ToString()))
                        .Where(x => x.MainProductId != null).ToList();
                    evalContext = evalContext.Clone() as PriceEvaluationContext;
                    evalContext.ProductIds = variations.Select(x => x.MainProductId).Distinct().ToArray();
                    if (!evalContext.ProductIds.IsNullOrEmpty())
                    {
                        var inheritedPrices = await EvaluateProductPricesAsync(evalContext);
                        foreach (var inheritedPrice in inheritedPrices)
                        {
                            foreach (var variation in variations.Where(x => x.MainProductId == inheritedPrice.ProductId))
                            {
                                var variationPrice = inheritedPrice.Clone() as Price;
                                //Reset id for correct override price in possible update 
                                variationPrice.Id = null;
                                variationPrice.ProductId = variation.Id;
                                result.Add(variationPrice);
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override Task<IEnumerable<PriceEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IPriceRepository)repository).GetByIdsAsync(ids);
        }

        protected override void ClearCache(IEnumerable<Price> models)
        {            
            GenericCachingRegion<Price>.ExpireRegion();
            base.ClearCache(models);
        }
    }
}
