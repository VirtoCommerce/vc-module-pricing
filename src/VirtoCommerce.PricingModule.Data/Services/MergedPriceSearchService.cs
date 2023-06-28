using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Model.Search;
using VirtoCommerce.CatalogModule.Core.Search;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class MergedPriceSearchService : IMergedPriceSearchService
    {
        private readonly Func<IPricingRepository> _repositoryFactory;
        private readonly IPricelistService _pricelistService;
        private readonly IItemService _itemService;
        private readonly IProductIndexedSearchService _productIndexedSearchService;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public MergedPriceSearchService(
            Func<IPricingRepository> repositoryFactory,
            IPricelistService pricelistService,
            IItemService itemService,
            IProductIndexedSearchService productIndexedSearchService,
            IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _pricelistService = pricelistService;
            _itemService = itemService;
            _productIndexedSearchService = productIndexedSearchService;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<MergedPriceGroupSearchResult> SearchGroupsAsync(MergedPriceSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchGroupsAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<Price>.CreateChangeToken());
                cacheEntry.AddExpirationToken(GenericSearchCachingRegion<Price>.CreateChangeToken());

                var result = AbstractTypeFactory<MergedPriceGroupSearchResult>.TryCreateInstance();

                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var query = await BuildQueryAsync(repository, criteria);

                    var groupedQuery = query.GroupBy(x => x.ProductId);

                    result.TotalCount = await groupedQuery.CountAsync();

                    if (criteria.Take > 0)
                    {
                        groupedQuery = groupedQuery
                        .OrderBy(x => x.Key)
                        .Skip(criteria.Skip)
                        .Take(criteria.Take);

                        var resultGroups = await groupedQuery.Select(x => new MergedPriceGroupEntity
                        {
                            ProductId = x.Key,
                            GroupPricesCount = x.Count(),
                            MinListPrice = x.Min(g => g.List),
                            MaxListPrice = x.Max(g => g.List),
                            MinSalePrice = x.Min(g => g.Sale),
                            MaxSalePrice = x.Max(g => g.Sale),
                            GroupState = x.Max(g => g.State)
                        }).ToListAsync();

                        result.Results = resultGroups
                        .Select(x => x.ToModel(AbstractTypeFactory<MergedPriceGroup>.TryCreateInstance()))
                        .ToList();
                    }
                }

                if (result.Results.Any())
                {
                    var productIds = result.Results.Select(x => x.ProductId).ToList();
                    var products = await _itemService.GetNoCloneAsync(productIds, ItemResponseGroup.ItemInfo.ToString());
                    foreach (var group in result.Results)
                    {
                        var product = products.FirstOrDefault(x => x.Id == group.ProductId);
                        if (product != null)
                        {
                            group.ProductCode = product.Code;
                            group.ProductName = product.Name;
                            group.ProductImgSrc = product.ImgSrc;
                        }
                    }
                }

                return result;
            });
        }

        public async Task<MergedPriceSearchResult> SearchGroupPricesAsync(MergedPriceSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchGroupPricesAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(GenericCachingRegion<Price>.CreateChangeToken());
                cacheEntry.AddExpirationToken(GenericSearchCachingRegion<Price>.CreateChangeToken());

                var result = AbstractTypeFactory<MergedPriceSearchResult>.TryCreateInstance();

                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();

                    var query = await BuildQueryAsync(repository, criteria);

                    if (criteria.All)
                    {
                        query = query.OrderBy(x => x.MinQuantity);
                    }
                    else
                    {
                        result.TotalCount = await query.CountAsync();

                        query = query
                            .OrderBy(x => x.ProductId)
                            .ThenBy(x => x.MinQuantity)
                            .Skip(criteria.Skip)
                            .Take(criteria.Take);
                    }

                    var results = await query.ToListAsync();

                    result.TotalCount = criteria.All ? results.Count : result.TotalCount;
                    result.Results = results
                        .Select(x => x.ToModel(AbstractTypeFactory<MergedPrice>.TryCreateInstance()))
                        .ToList();
                }

                if (result.Results.Any())
                {
                    // get currency from base pricelist
                    var priceList = await _pricelistService.GetNoCloneAsync(criteria.BasePriceListId, PriceListResponseGroup.NoDetails.ToString());
                    if (priceList != null)
                    {
                        foreach (var price in result.Results)
                        {
                            price.Currency = priceList.Currency;
                        }
                    }
                }

                return result;
            });
        }

        protected async Task<IQueryable<MergedPriceEntity>> BuildQueryAsync(IPricingRepository repository, MergedPriceSearchCriteria criteria)
        {
            var query = repository.GetMergedPrices(criteria.BasePriceListId, criteria.PriorityPriceListId);

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ProductIds.Contains(x.ProductId));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var searchCriteria = AbstractTypeFactory<ProductIndexedSearchCriteria>.TryCreateInstance();
                searchCriteria.Keyword = criteria.Keyword;
                searchCriteria.Skip = criteria.Skip;
                searchCriteria.Take = criteria.Take;
                searchCriteria.ResponseGroup = ItemResponseGroup.ItemInfo.ToString();
                searchCriteria.WithHidden = true;
                var searchResult = await _productIndexedSearchService.SearchAsync(searchCriteria);

                var productIds = searchResult.Items.Select(x => x.Id);

                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            return query;
        }
    }
}
