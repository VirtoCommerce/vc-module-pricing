using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PriceSearchService : SearchService<PricesSearchCriteria, PriceSearchResult, Price, PriceEntity>, IPriceSearchService
    {
        /// <summary>
        /// Limits counts of products that can be found by keyword for price search
        /// </summary>
        private const int _maxSearchProductByKeywordResults = 1000;

        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IPriceService _crudService;
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly Dictionary<string, string> _pricesSortingAliases = new();

        public PriceSearchService(
            Func<IPricingRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IPriceService crudService,
            IOptions<CrudOptions> crudOptions,
            IProductIndexedSearchService productIndexedSearchService)
           : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
        {
            _pricesSortingAliases["prices"] = ReflectionUtility.GetPropertyName<Price>(x => x.List);
            _repositoryFactory = repositoryFactory;
            _platformMemoryCache = platformMemoryCache;
            _crudService = crudService;
            _productIndexedSearchService = productIndexedSearchService;
        }

        public override Task<PriceSearchResult> SearchAsync(PricesSearchCriteria criteria, bool clone = true)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchAsync), criteria.GetCacheKey());
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<Price>.CreateChangeToken());
                cacheEntry.AddExpirationToken(GenericSearchCachingRegion<Price>.CreateChangeToken());

                var result = AbstractTypeFactory<PriceSearchResult>.TryCreateInstance();

                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    var query = await BuildQueryAsync(repository, criteria);
                    var sortInfos = BuildSortExpression(criteria);
                    //Try to replace sorting columns names
                    TryTransformSortingInfoColumnNames(_pricesSortingAliases, sortInfos);

                    if (criteria.GroupByProducts)
                    {
                        // Note: GroupByProducts breaks the products sorting
                        var allProductIdsQuery = query.Select(x => x.ProductId).Distinct().OrderBy(x => x);

                        result.TotalCount = await allProductIdsQuery.CountAsync();

                        if (criteria.Take > 0)
                        {
                            var pagedProductIds = await allProductIdsQuery
                                .OrderBy(x => x)
                                .Skip(criteria.Skip)
                                .Take(criteria.Take)
                                .ToListAsync();

                            var priceIds = await query
                                .Where(x => pagedProductIds.Contains(x.ProductId))
                                .OrderBySortInfos(sortInfos)
                                .ThenBy(x => x.Id)
                                .Select(x => x.Id)
                                .AsNoTracking()
                                .ToListAsync();

                            var unorderedResults = await _crudService.GetAsync(priceIds, responseGroup: null, clone);

                            result.Results = unorderedResults
                                .OrderBy(x => pagedProductIds.IndexOf(x.ProductId))
                                .ThenBy(x => x.Id)
                                .ToList();
                        }
                    }
                    else
                    {
                        result.TotalCount = await query.CountAsync();

                        if (criteria.Take > 0)
                        {
                            var priceIds = await query
                                .OrderBySortInfos(sortInfos)
                                .ThenBy(x => x.Id)
                                .Skip(criteria.Skip)
                                .Take(criteria.Take)
                                .Select(x => x.Id)
                                .AsNoTracking()
                                .ToListAsync();

                            var unorderedResults = await _crudService.GetAsync(priceIds, responseGroup: null, clone);

                            result.Results = unorderedResults.OrderBy(x => priceIds.IndexOf(x.Id)).ToList();
                        }
                    }
                }

                return result;
            });
        }

        protected override IQueryable<PriceEntity> BuildQuery(IRepository repository, PricesSearchCriteria criteria)
        {
            return BuildQueryAsync(repository, criteria).GetAwaiter().GetResult();
        }

        protected override IList<SortInfo> BuildSortExpression(PricesSearchCriteria criteria)
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

        private async Task<IQueryable<PriceEntity>> BuildQueryAsync(IRepository repository, PricesSearchCriteria criteria)
        {
            var query = ((IPricingRepository)repository).Prices;

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
                searchCriteria.Skip = 0;
                searchCriteria.Take = _maxSearchProductByKeywordResults;
                searchCriteria.Sort = criteria.Sort.Replace("product.", string.Empty);
                searchCriteria.ResponseGroup = ItemResponseGroup.ItemInfo.ToString();
                searchCriteria.WithHidden = true;
                var searchResult = await _productIndexedSearchService.SearchAsync(searchCriteria);

                var productIds = searchResult.Items.Select(x => x.Id);

                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            return query;
        }

        private static void TryTransformSortingInfoColumnNames(IDictionary<string, string> transformationMap, IEnumerable<SortInfo> sortingInfos)
        {
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
