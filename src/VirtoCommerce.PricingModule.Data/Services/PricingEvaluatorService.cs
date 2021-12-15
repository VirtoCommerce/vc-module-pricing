using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Repositories;

#pragma warning disable CS0618 // Allow to use obsoleted

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingEvaluatorService : IPricingEvaluatorService
    {
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly ILogger<PricingEvaluatorService> _logger;
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;
        private readonly IItemService _productService;

        public PricingEvaluatorService(
                Func<IPricingRepository> repositoryFactory,
                IItemService productService,
                ILogger<PricingEvaluatorService> logger,
                IPlatformMemoryCache platformMemoryCache,
                IPricingPriorityFilterPolicy pricingPriorityFilterPolicy
            )
        {
            _platformMemoryCache = platformMemoryCache;
            _repositoryFactory = repositoryFactory;
            _logger = logger;
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
            _productService = productService;
        }

        public virtual async Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            List<PricelistAssignment> assignmentsToReturn;
            var query = await PriceListAssignmentAsync(evalContext);
            if (evalContext.SkipAssignmentValidation)
            {
                assignmentsToReturn = query.ToList();
            }
            else
            {
                var assignments = query.ToList();
                assignmentsToReturn = assignments.Where(x => x.DynamicExpression == null).ToList();

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
            }

            return assignmentsToReturn.OrderByDescending(x => x.Priority).ThenByDescending(x => x.Name).Select(x => x.Pricelist);
        }

        public virtual async Task<IQueryable<PricelistAssignment>> PriceListAssignmentAsync(PriceEvaluationContext evalContext)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(EvaluatePriceListsAsync));
            var priceListAssignments = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<PricelistAssignment>.CreateChangeToken());

                return await GetAllPricelistAssignments();
            });

            var query = priceListAssignments.AsQueryable();

            if (evalContext.StoreId != null)
            {
                query = query.Where(x => x.StoreId == evalContext.StoreId);
            }

            if (evalContext.CatalogId != null)
            {
                query = query.Where(x => x.CatalogId == evalContext.CatalogId);
            }

            if (evalContext.Currency != null)
            {
                query = query.Where(x => x.Pricelist.Currency == evalContext.Currency.ToString());
            }

            if (evalContext.CertainDate != null)
            {
                query = query.Where(x => (x.StartDate == null || evalContext.CertainDate >= x.StartDate) && (x.EndDate == null || x.EndDate >= evalContext.CertainDate));
            }

            return query;
        }

        public virtual async Task<PricelistAssignment[]> GetAllPricelistAssignments()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                return (await repository.PricelistAssignments.Include(x => x.Pricelist).AsNoTracking().ToListAsync())
                    .Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
            }
        }

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
            IEnumerable<Price> prices;
            using (var repository = _repositoryFactory())
            {
                //Get a price range satisfying by passing context
                var query = (repository).Prices.Include(x => x.Pricelist)
                                             .Where(x => evalContext.ProductIds.Contains(x.ProductId))
                                             .Where(x => evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0);

                evalContext.PricelistIds = evalContext.PricelistIds.IsNullOrEmpty() ? (await EvaluatePriceListsAsync(evalContext)).Select(x => x.Id).ToArray()
                                                                                    : evalContext.PricelistIds;

                query = query.Where(x => evalContext.PricelistIds.Contains(x.PricelistId));

                // Filter by date expiration
                // Always filter on date, so that we limit the results to process.
                var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;
                query = query.Where(x => (x.StartDate == null || x.StartDate <= certainDate)
                    && (x.EndDate == null || x.EndDate > certainDate));

                var queryResult = await query.AsNoTracking().ToListAsync();
                prices = queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance()));
            }

            //Apply pricing filtration strategy for found prices
            result.AddRange(_pricingPriorityFilterPolicy.FilterPrices(prices, evalContext));

            if (_productService == null)
            {
                return result;
            }
            //Then variation inherited prices
            var productIdsWithoutPrice = evalContext.ProductIds.Except(result.Select(x => x.ProductId).Distinct());
            if (!productIdsWithoutPrice.Any())
            {
                return result;
            }

            //Try to inherit prices for variations from their main product
            //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
            var variations = (await _productService.GetByIdsAsync(productIdsWithoutPrice.ToArray(), ItemResponseGroup.ItemInfo.ToString()))
                .Where(x => x.MainProductId != null).ToList();
            evalContext = evalContext.Clone() as PriceEvaluationContext;
            evalContext.ProductIds = variations.Select(x => x.MainProductId).Distinct().ToArray();

            if (evalContext.ProductIds.IsNullOrEmpty())
            {
                return result;
            }

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

            return result;
        }
    }
}
