using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly Dictionary<string, string> _pricesSortingAliases = new Dictionary<string, string>();

        public PriceSearchService(Func<IPriceRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache,
            IPriceService priceService, IProductIndexedSearchService productIndexedSearchService)
           : base(repositoryFactory, platformMemoryCache, (ICrudService<Price>)priceService)
        {
            _pricesSortingAliases["prices"] = ReflectionUtility.GetPropertyName<Price>(x => x.List);
            _productIndexedSearchService = productIndexedSearchService;
        }

        public override Task<PriceSearchResult> SearchAsync(PricesSearchCriteria criteria)
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

                        var unorderedResults = await _crudService.GetByIdsAsync(priceIds);
                        result.Results = unorderedResults.OrderBy(x => Array.IndexOf(priceIds, x.Id)).ToList();
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
            var query = ((IPriceRepository)repository).Prices;

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
