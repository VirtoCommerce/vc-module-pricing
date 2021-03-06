using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Caching;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricingSearchServiceImpl : IPricingSearchService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IPricingService _pricingService;
        private readonly Dictionary<string, string> _pricesSortingAliases = new Dictionary<string, string>();
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IProductIndexedSearchService _productIndexedSearchService;

        public PricingSearchServiceImpl(Func<IPricingRepository> repositoryFactory, IPricingService pricingService
            , IPlatformMemoryCache platformMemoryCache
            , IProductIndexedSearchService productIndexedSearchService)
        {
            _repositoryFactory = repositoryFactory;
            _pricesSortingAliases["prices"] = ReflectionUtility.GetPropertyName<Price>(x => x.List);
            _pricingService = pricingService;
            _platformMemoryCache = platformMemoryCache;
            _productIndexedSearchService = productIndexedSearchService;
        }


        #region IPricingSearchService Members

        public virtual Task<PriceSearchResult> SearchPricesAsync(PricesSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchPricesAsync), criteria.GetCacheKey());
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(PricesCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(PricingSearchCacheRegion.CreateChangeToken());

                var result = AbstractTypeFactory<PriceSearchResult>.TryCreateInstance();

                using (var repository = _repositoryFactory())
                {
                    var query = await BuildQueryAsync(repository, criteria);
                    var sortInfos = BuildSortExpression(criteria);
                    //Try to replace sorting columns names
                    TryTransformSortingInfoColumnNames(_pricesSortingAliases, sortInfos);

                    if (criteria.GroupByProducts)
                    {
                        // unique priced product IDs
                        var pricedProductsQuery = query.Select(x => x.ProductId).OrderBy(x => x).Distinct();
                        result.TotalCount = await pricedProductsQuery.CountAsync();

                        if (criteria.Take > 0)
                        {
                            query = query.Where(x => pricedProductsQuery
                                                        .OrderBy(x => x)
                                                        .Skip(criteria.Skip).Take(criteria.Take).Contains(x.ProductId));
                        }
                    }
                    else
                    {
                        result.TotalCount = await query.CountAsync();

                        query = query.Skip(criteria.Skip).Take(criteria.Take);
                    }

                    if (criteria.Take > 0)
                    {
                        var priceIds = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                                    .Select(x => x.Id)
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                        var unorderedResults = await _pricingService.GetPricesByIdAsync(priceIds);
                        result.Results = unorderedResults.OrderBy(x => Array.IndexOf(priceIds, x.Id)).ToList();
                    }
                }
                return result;
            });
        }

        public virtual async Task<PricelistSearchResult> SearchPricelistsAsync(PricelistSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchPricelistsAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(PricingSearchCacheRegion.CreateChangeToken());

                var result = AbstractTypeFactory<PricelistSearchResult>.TryCreateInstance();
                using (var repository = _repositoryFactory())
                {
                    var query = BuildQuery(repository, criteria);
                    var sortInfos = BuildSortExpression(criteria);

                    result.TotalCount = await query.CountAsync();

                    if (criteria.Take > 0)
                    {
                        var pricelistIds = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                                                .Select(x => x.Id)
                                                                .Skip(criteria.Skip).Take(criteria.Take)
                                                                .AsNoTracking()
                                                                .ToArrayAsync();
                        var unorderedResults = await _pricingService.GetPricelistsByIdAsync(pricelistIds);
                        result.Results = unorderedResults.OrderBy(x => Array.IndexOf(pricelistIds, x.Id)).ToList();
                    }
                }

                return result;
            });
        }

        public virtual async Task<PricelistAssignmentSearchResult> SearchPricelistAssignmentsAsync(PricelistAssignmentsSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchPricelistAssignmentsAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(PricingSearchCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(PricingCacheRegion.CreateChangeToken());

                var result = AbstractTypeFactory<PricelistAssignmentSearchResult>.TryCreateInstance();
                using (var repository = _repositoryFactory())
                {
                    var query = BuildQuery(repository, criteria);
                    var sortInfos = BuildSortExpression(criteria);

                    result.TotalCount = await query.CountAsync();

                    if (criteria.Take > 0)
                    {
                        var pricelistAssignmentsIds = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                                                 .Select(x => x.Id)
                                                                .Skip(criteria.Skip).Take(criteria.Take)
                                                                .AsNoTracking()
                                                                .ToArrayAsync();
                        var unorderedResults = await _pricingService.GetPricelistAssignmentsByIdAsync(pricelistAssignmentsIds);
                        result.Results = unorderedResults.OrderBy(x => Array.IndexOf(pricelistAssignmentsIds, x.Id)).ToList();
                    }
                }
                return result;
            });
        }
        #endregion
        protected virtual async Task<IQueryable<PriceEntity>> BuildQueryAsync(IPricingRepository repository, PricesSearchCriteria criteria)
        {
            var query = repository.Prices;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
            }

            if (criteria.ModifiedSince.HasValue)
            {
                query = query.Where(x => x.ModifiedDate >= criteria.ModifiedSince);
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var searchCriteria = AbstractTypeFactory<ProductIndexedSearchCriteria>.TryCreateInstance();
                searchCriteria.Keyword = criteria.Keyword;
                searchCriteria.Skip = criteria.Skip;
                searchCriteria.Take = criteria.Take;
                searchCriteria.Sort = criteria.Sort.Replace("product.", string.Empty);
                searchCriteria.ResponseGroup = ItemResponseGroup.ItemInfo.ToString();
                searchCriteria.WithHidden = true;
                var searchResult = await _productIndexedSearchService.SearchAsync(searchCriteria);

                var productIds = searchResult.Items.Select(x => x.Id).ToArray();

                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(PricesSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PriceEntity.List)
                    }
                };
            }
            return sortInfos;
        }

        protected virtual IQueryable<PricelistEntity> BuildQuery(IPricingRepository repository, PricelistSearchCriteria criteria)
        {
            var query = repository.Pricelists;

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.Currencies.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.Currencies.Contains(x.Currency));
            }

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(PricelistSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PricelistEntity.Name)
                    }
                };
            }
            return sortInfos;
        }

        protected virtual IQueryable<PricelistAssignmentEntity> BuildQuery(IPricingRepository repository, PricelistAssignmentsSearchCriteria criteria)
        {
            var query = repository.PricelistAssignments;

            if (!criteria.PriceListIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.PriceListIds.Contains(x.PricelistId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword) || x.Description.Contains(criteria.Keyword));
            }

            if (!criteria.CatalogIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.CatalogIds.Contains(x.CatalogId));
            }

            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(PricelistAssignmentsSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo
                    {
                        SortColumn = nameof(PricelistAssignment.Priority)
                    }
                };
            }
            return sortInfos;
        }

        private static void TryTransformSortingInfoColumnNames(IDictionary<string, string> transformationMap, IEnumerable<SortInfo> sortingInfos)
        {
            //Try to replace sorting columns names
            foreach (var sortInfo in sortingInfos)
            {
                if (transformationMap.TryGetValue(sortInfo.SortColumn.ToLowerInvariant(), out var newColumnName))
                {
                    sortInfo.SortColumn = newColumnName;
                }
            }
        }
    }
}

