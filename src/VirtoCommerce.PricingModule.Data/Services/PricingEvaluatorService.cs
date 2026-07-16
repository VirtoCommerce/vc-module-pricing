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
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingEvaluatorService : IPricingEvaluatorService
    {
        private const int _priceQueryChunkSize = 500;

        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly ILogger<PricingEvaluatorService> _logger;
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;
        private readonly IItemService _productService;
        private readonly ISettingsManager _settingsManager;

        public PricingEvaluatorService(
                Func<IPricingRepository> repositoryFactory,
                IItemService productService,
                ILogger<PricingEvaluatorService> logger,
                IPlatformMemoryCache platformMemoryCache,
                IPricingPriorityFilterPolicy pricingPriorityFilterPolicy,
                ISettingsManager settingsManager = null
            )
        {
            _platformMemoryCache = platformMemoryCache;
            _repositoryFactory = repositoryFactory;
            _logger = logger;
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
            _productService = productService;
            _settingsManager = settingsManager;
        }

        protected virtual Task<bool> IsEvaluatorCacheEnabledAsync()
        {
            return _settingsManager == null
                ? Task.FromResult(true)
                : _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.PriceEvaluationCacheEnabled);
        }

        public virtual async Task<IList<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext)
        {
            List<PricelistAssignment> assignmentsToReturn;
            var query = await PriceListAssignmentAsync(evalContext);
            if (evalContext.SkipAssignmentValidation)
            {
                // do NOT use ToListAsync as "query" is not EF IAsyncQueryable
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

            return assignmentsToReturn
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => x.Name)
                .Select(x =>
                {
                    x.Pricelist.Priority = x.Priority;
                    return x.Pricelist;
                })
                .ToList();
        }

        public virtual async Task<IQueryable<PricelistAssignment>> PriceListAssignmentAsync(PriceEvaluationContext evalContext)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(EvaluatePriceListsAsync));
            var priceListAssignments = await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<PricelistAssignment>.CreateChangeToken());

                return await GetAllPricelistAssignments();
            });

            // Not .AsQueryable().Where(...): that recompiles the expression tree per enumeration, a lock convoy under load.
            IEnumerable<PricelistAssignment> assignments = priceListAssignments;

            if (evalContext.StoreId != null || evalContext.CatalogId != null)
            {
                assignments = assignments.Where(x => MatchesScope(x, evalContext));
            }

            if (evalContext.Currency != null)
            {
                assignments = assignments.Where(x => x.Pricelist.Currency == evalContext.Currency);
            }

            if (evalContext.CertainDate != null)
            {
                assignments = assignments.Where(x => MatchesDate(x, evalContext.CertainDate.Value));
            }

            return assignments.AsQueryable();
        }

        private static bool MatchesScope(PricelistAssignment assignment, PriceEvaluationContext evalContext)
        {
            return (evalContext.StoreId != null && assignment.StoreId == evalContext.StoreId)
                || (evalContext.CatalogId != null && assignment.CatalogId == evalContext.CatalogId);
        }

        private static bool MatchesDate(PricelistAssignment assignment, DateTime certainDate)
        {
            return (assignment.StartDate == null || certainDate >= assignment.StartDate)
                && (assignment.EndDate == null || assignment.EndDate >= certainDate);
        }

        public virtual async Task<PricelistAssignment[]> GetAllPricelistAssignments()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                return (await repository.PricelistAssignments
                    .Include(x => x.Pricelist).AsSingleQuery().AsNoTracking().ToListAsync())
                    .Select(x => x.ToModel(AbstractTypeFactory<PricelistAssignment>.TryCreateInstance())).ToArray();
            }
        }

        public virtual async Task<IList<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext)
        {
            if (evalContext == null)
            {
                throw new ArgumentNullException(nameof(evalContext));
            }
            if (evalContext.ProductIds == null)
            {
                throw new MissingFieldException("ProductIds");
            }

            if (evalContext.PricelistIds.IsNullOrEmpty())
            {
                evalContext.Pricelists = evalContext.Pricelists.IsNullOrEmpty()
                    ? (await EvaluatePriceListsAsync(evalContext)).ToArray()
                    : evalContext.Pricelists;

                evalContext.PricelistIds = evalContext.Pricelists.Select(x => x.Id).ToArray();
            }

            var rawPrices = await LoadPricesFromDatabaseAsync(evalContext.ProductIds, evalContext.PricelistIds);
            var prices = ApplyQuantityAndDateFilter(rawPrices, evalContext);

            var result = new List<Price>();
            result.AddRange(await PostProcessPrices(evalContext, prices));

            return result;
        }

        protected virtual async Task<IList<Price>> LoadPricesFromDatabaseAsync(IList<string> productIds, IList<string> pricelistIds)
        {
            var result = new List<Price>();
            using (var repository = _repositoryFactory())
            {
                foreach (var productIdsChunk in productIds.Chunk(_priceQueryChunkSize))
                {
                    var queryResult = await repository.Prices
                        .Include(x => x.Pricelist).AsSingleQuery()
                        .Where(x => productIdsChunk.Contains(x.ProductId))
                        .Where(x => pricelistIds.Contains(x.PricelistId))
                        .AsNoTracking().ToListAsync();

                    result.AddRange(queryResult.Select(x => x.ToModel(AbstractTypeFactory<Price>.TryCreateInstance())));
                }
            }

            return result;
        }

        private static IEnumerable<Price> ApplyQuantityAndDateFilter(IEnumerable<Price> prices, PriceEvaluationContext evalContext)
        {
            // Reproduces the former SQL WHERE (PricingEvaluatorService.cs:166 and :181-183) in memory,
            // so cached rows can stay unfiltered (AC-5b).
            var certainDate = evalContext.CertainDate ?? DateTime.UtcNow;

            return prices.Where(x => (evalContext.Quantity >= x.MinQuantity || evalContext.Quantity == 0)
                && (x.StartDate == null || x.StartDate <= certainDate)
                && (x.EndDate == null || x.EndDate > certainDate));
        }

        private async Task<List<Price>> PostProcessPrices(PriceEvaluationContext evalContext, IEnumerable<Price> prices)
        {
            var result = new List<Price>();

            result.AddRange(_pricingPriorityFilterPolicy.FilterPrices(prices, evalContext));

            if (_productService == null)
            {
                return result;
            }
            //Then variation inherited prices
            var productIdsWithoutPrice = evalContext.ProductIds.Except(result.Select(x => x.ProductId).Distinct()).ToArray();
            if (!productIdsWithoutPrice.Any())
            {
                return result;
            }

            //Try to inherit prices for variations from their main product
            //Need find products without price it may be a variation without implicitly price defined and try to get price from main product
            var variations = (await _productService.GetNoCloneAsync(productIdsWithoutPrice, ItemResponseGroup.ItemInfo.ToString()))
                .Where(x => x.MainProductId != null).ToList();
            evalContext = evalContext.CloneTyped();
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
                    var variationPrice = inheritedPrice.CloneTyped();
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
